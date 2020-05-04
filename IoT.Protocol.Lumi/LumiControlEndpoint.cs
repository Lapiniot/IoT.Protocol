using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;
using static System.Net.Sockets.ProtocolType;
using static System.Net.Sockets.SocketFlags;
using static System.Net.Sockets.SocketType;
using static System.StringComparison;

namespace IoT.Protocol.Lumi
{
    public sealed class LumiControlEndpoint : ActivityObject, IConnectedEndpoint<IDictionary<string, object>, JsonElement>
    {
        private const int MaxReceiveBufferSize = 2048;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> completions =
            new ConcurrentDictionary<string, TaskCompletionSource<JsonElement>>();

        private readonly IPEndPoint endpoint;
        private Task dispatchTask;
        private Socket socket;
        private CancellationTokenSource tokenSource;

        public LumiControlEndpoint(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        private TimeSpan CommandTimeout { get; } = TimeSpan.FromSeconds(10);

        #region Implementation of IControlEndpoint<in IDictionary<string,object>, JsonElement>

        public async Task<JsonElement> InvokeAsync(IDictionary<string, object> message, CancellationToken cancellationToken)
        {
            if(message == null) throw new ArgumentNullException(nameof(message));

            var completionSource = new TaskCompletionSource<JsonElement>(cancellationToken);

            var (id, datagram) = (GetCommandKey((string)message["cmd"], (string)message["sid"]), JsonSerializer.SerializeToUtf8Bytes(message));

            try
            {
                completions.TryAdd(id, completionSource);

                var vt = socket.SendAsync(datagram, None, cancellationToken);
                if(!vt.IsCompletedSuccessfully)
                {
                    await vt.ConfigureAwait(false);
                }

                using var timeoutSource = new CancellationTokenSource(CommandTimeout);

                return await completionSource.Task.WaitAsync(timeoutSource.Token).ConfigureAwait(false);
            }
            catch(OperationCanceledException)
            {
                completionSource.TrySetCanceled();
                throw;
            }
            finally
            {
                completions.TryRemove(id, out _);
            }
        }

        #endregion

        private static bool TryParseResponse(Span<byte> span, out string id, out JsonElement response)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(span);

            if(json.TryGetProperty("cmd", out var cmd) && json.TryGetProperty("sid", out var sid))
            {
                id = GetCommandKey(GetCmdName(cmd.GetString()), sid.GetString());
                response = json;
                return true;
            }

            id = null;
            response = default;
            return false;
        }

        private static string GetCommandKey(string command, string sid)
        {
            return $"{command}.{sid}";
        }

        private static string GetCmdName(string command)
        {
            return command.EndsWith("_ack", InvariantCulture) ? command[..^4] : command;
        }

        public Task<JsonElement> InvokeAsync(string command, string sid, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(new Dictionary<string, object> {{"cmd", command}, {"sid", sid}}, cancellationToken);
        }

        private void OnDataAvailable(Span<byte> span)
        {
            if(!TryParseResponse(span, out var id, out var response)) return;

            if(!completions.TryRemove(id, out var completionSource)) return;

            completionSource.TrySetResult(response);
        }

        private async Task DispatchAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[MaxReceiveBufferSize];

            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var vt = socket.ReceiveAsync(buffer, None, cancellationToken);

                    var size = vt.IsCompletedSuccessfully ? vt.Result : await vt.ConfigureAwait(false);

                    OnDataAvailable(buffer.AsSpan(0, size));
                }
                catch(OperationCanceledException)
                {
                    Trace.TraceInformation("Cancelling message dispatching loop...");
                }
                catch(Exception e)
                {
                    Trace.TraceError($"Error in message dispatch: {e.Message}");
                    throw;
                }
            }
        }

        #region Overrides of ActivityObject

        protected override async Task StartingAsync(CancellationToken cancellationToken)
        {
            socket = new Socket(endpoint.AddressFamily, Dgram, Udp);

            await socket.ConnectAsync(endpoint).ConfigureAwait(false);

            tokenSource = new CancellationTokenSource();

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

        #endregion

        #region Implementation of IConnectedObject

        public bool IsConnected => IsRunning;

        public Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            return StartActivityAsync(cancellationToken);
        }

        public Task DisconnectAsync()
        {
            return StopActivityAsync();
        }

        #endregion
    }
}