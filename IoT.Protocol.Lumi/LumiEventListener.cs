using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace IoT.Protocol.Lumi;

public sealed class LumiEventListener : ActivityObject, IObservable<JsonElement>
{
    private const int MaxReceiveBufferSize = 2048;
    private readonly IPEndPoint endpoint;
    private readonly ObserversContainer<JsonElement> observers;
    private Socket socket;
    private CancellationTokenSource tokenSource;

    public LumiEventListener(IPEndPoint groupEndpoint)
    {
        endpoint = groupEndpoint;
        observers = new ObserversContainer<JsonElement>();
    }

    #region Implementation of IObservable<out JsonElement>

    public IDisposable Subscribe(IObserver<JsonElement> observer) => observers.Subscribe(observer);

    #endregion

    #region Overrides of ActivityObject

    public override async ValueTask DisposeAsync()
    {
        using (observers)
        using (socket)
        using (tokenSource)
        {
            await base.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion

    private async Task DispatchAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[MaxReceiveBufferSize];

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await socket.ReceiveFromAsync(buffer, default, endpoint).WaitAsync(cancellationToken).ConfigureAwait(false);

                var message = JsonSerializer.Deserialize<JsonElement>(buffer.AsSpan(0, result.ReceivedBytes));

                observers.Notify(message);
            }
            catch (OperationCanceledException)
            {
                Trace.TraceInformation("Cancelling message dispatching loop...");
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error in message dispatch: {e.Message}");
                throw;
            }
        }
    }

    public Task ConnectAsync(in CancellationToken cancellationToken) => StartActivityAsync(cancellationToken);

    public Task DisconnectAsync() => StopActivityAsync();

    #region Overrides of ActivityObject

    protected override Task StartingAsync(CancellationToken cancellationToken)
    {
        socket = SocketBuilderExtensions.CreateUdp();

        socket.JoinMulticastGroup(endpoint);

        tokenSource = new CancellationTokenSource();

        var token = tokenSource.Token;

        _ = Task.Run(() => DispatchAsync(token), token);

        return Task.CompletedTask;
    }

    protected override Task StoppingAsync()
    {
        var source = tokenSource;

        if (source != null)
        {
            source.Cancel();
            source.Dispose();
        }

        tokenSource = null;

        socket.Close();

        return Task.CompletedTask;
    }

    #endregion
}