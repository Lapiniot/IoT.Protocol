using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using OOs;
using OOs.Net.Sockets;

namespace IoT.Protocol.Lumi;

public sealed class LumiEventListener(IPEndPoint groupEndpoint) : ActivityObject, IObservable<JsonElement>
{
    private const int MaxReceiveBufferSize = 2048;
    private readonly ObserversContainer<JsonElement> observers = new();
    private Socket socket;
    private CancellationTokenSource tokenSource;

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
                var result = await socket.ReceiveFromAsync(buffer, default, groupEndpoint).WaitAsync(cancellationToken).ConfigureAwait(false);

                var message = JsonSerializer.Deserialize(buffer.AsSpan(0, result.ReceivedBytes), JsonContext.Default.JsonElement);

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

        socket.JoinMulticastGroup(groupEndpoint);

        tokenSource = new();

        var token = tokenSource.Token;

        _ = Task.Run(() => DispatchAsync(token), token);

        return Task.CompletedTask;
    }

    protected override async Task StoppingAsync()
    {
        var source = tokenSource;

        if (source != null)
        {
            await source.CancelAsync().ConfigureAwait(false);
            source.Dispose();
        }

        tokenSource = null;

        socket.Close();
    }

    #endregion
}