using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using IoT.Protocol.Interfaces;
using static System.Net.Sockets.ProtocolType;
using static System.Net.Sockets.SocketFlags;
using static System.Net.Sockets.SocketType;
using static System.StringComparison;

namespace IoT.Protocol.Lumi;

public sealed class LumiControlEndpoint(IPEndPoint endpoint) : ActivityObject, IConnectedEndpoint<IDictionary<string, object>, JsonElement>
{
    private const int MaxReceiveBufferSize = 2048;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> completions = new();
    private Task dispatchTask;
    private Socket socket;
    private CancellationTokenSource tokenSource;

    private TimeSpan CommandTimeout { get; } = TimeSpan.FromSeconds(10);

    private static bool TryParseResponse(Span<byte> span, out string id, out JsonElement response)
    {
        var json = JsonSerializer.Deserialize(span, JsonContext.Default.JsonElement);

        if (json.TryGetProperty("cmd", out var cmd) && json.TryGetProperty("sid", out var sid))
        {
            id = GetCommandKey(GetCmdName(cmd.GetString()), sid.GetString());
            response = json;
            return true;
        }

        id = null;
        response = default;
        return false;
    }

    private static string GetCommandKey(string command, string sid) => $"{command}.{sid}";

    private static string GetCmdName(string command) => command.EndsWith("_ack", InvariantCulture) ? command[..^4] : command;

    public Task<JsonElement> InvokeAsync(string command, string sid, CancellationToken cancellationToken = default) =>
        InvokeAsync(new Dictionary<string, object> { { "cmd", command }, { "sid", sid } }, cancellationToken);

    private void OnDataAvailable(Span<byte> span)
    {
        if (!TryParseResponse(span, out var id, out var response)) return;

        if (!completions.TryRemove(id, out var completionSource)) return;

        completionSource.TrySetResult(response);
    }

    private async Task DispatchAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[MaxReceiveBufferSize];

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var size = await socket.ReceiveAsync(buffer, None, cancellationToken).ConfigureAwait(false);

                OnDataAvailable(buffer.AsSpan(0, size));
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

    #region Implementation of IControlEndpoint<in IDictionary<string,object>, JsonElement>

    public async Task<JsonElement> InvokeAsync(IDictionary<string, object> command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var completionSource = new TaskCompletionSource<JsonElement>(cancellationToken);

        var (id, datagram) = (GetCommandKey((string)command["cmd"], (string)command["sid"]),
            JsonSerializer.SerializeToUtf8Bytes(command, JsonContext.Default.IDictionaryStringObject));

        try
        {
            completions.TryAdd(id, completionSource);

            await socket.SendAsync(datagram, None, cancellationToken).ConfigureAwait(false);

            using var timeoutSource = new CancellationTokenSource(CommandTimeout);

            return await completionSource.Task.WaitAsync(timeoutSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            completionSource.TrySetCanceled(cancellationToken);
            throw;
        }
        finally
        {
            completions.TryRemove(id, out _);
        }
    }

    #endregion

    #region Overrides of ActivityObject

    protected override async Task StartingAsync(CancellationToken cancellationToken)
    {
        socket = new(endpoint.AddressFamily, Dgram, Udp);

        await socket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

        tokenSource = new();

        var token = tokenSource.Token;

        dispatchTask = DispatchAsync(token);
    }

    protected override async Task StoppingAsync()
    {
        using var source = tokenSource;

        tokenSource = null;

        source.Cancel();

        await dispatchTask.ConfigureAwait(false);

        socket.Shutdown(SocketShutdown.Both);

        socket.Close();
    }

    public sealed override async ValueTask DisposeAsync()
    {
        using (tokenSource)
        using (socket)
        {
            await base.DisposeAsync().ConfigureAwait(false);
        }
    }

    #endregion

    #region Implementation of IConnectedObject

    public bool IsConnected => IsRunning;

    public Task ConnectAsync(CancellationToken cancellationToken = default) => StartActivityAsync(cancellationToken);

    public Task DisconnectAsync() => StopActivityAsync();

    #endregion
}