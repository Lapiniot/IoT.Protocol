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

namespace IoT.Protocol.Lumi
{
    public sealed class LumiControlEndpoint : ActivityObject, IConnectedEndpoint<IDictionary<string, object>, IDictionary<string, object>>
    {
        private const int ReceiveBufferSize = 0x8000;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<IDictionary<string, object>>> completions =
            new ConcurrentDictionary<string, TaskCompletionSource<IDictionary<string, object>>>();

        private readonly IPEndPoint endpoint;
        private Task dispatchTask;
        private Socket socket;
        private CancellationTokenSource tokenSource;

        public LumiControlEndpoint(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        private TimeSpan CommandTimeout { get; } = TimeSpan.FromSeconds(10);

        #region Implementation of IControlEndpoint<in IDictionary<string,object>,IDictionary<string,object>>

        public async Task<IDictionary<string, object>> InvokeAsync(IDictionary<string, object> message, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<IDictionary<string, object>>(cancellationToken);

            var (id, datagram) = (GetCommandKey((string)message["cmd"], (string)message["sid"]), JsonSerializer.SerializeToUtf8Bytes(message));

            try
            {
                completions.TryAdd(id, completionSource);

                var vt = socket.SendAsync(datagram, None, cancellationToken);
                if(!vt.IsCompletedSuccessfully)
                {
                    await vt.AsTask().ConfigureAwait(false);
                }

                using(var timeoutSource = new CancellationTokenSource(CommandTimeout))
                using(completionSource.Bind(cancellationToken, timeoutSource.Token))
                {
                    return await completionSource.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                completions.TryRemove(id, out _);
            }
        }

        #endregion

        private static bool TryParseResponse(byte[] buffer, int size, out string id, out IDictionary<string, object> response)
        {
            var json = JsonSerializer.Deserialize<IDictionary<string, object>>(buffer[..size]);
            if(json.TryGetValue("cmd", out var cmd) && json.TryGetValue("sid", out var sid))
            {
                id = GetCommandKey(GetCmdName((string)cmd), (string)sid);
                response = json;
                return true;
            }

            id = null;
            response = null;
            return false;
        }

        private static string GetCommandKey(string command, string sid)
        {
            return $"{command}.{sid}";
        }

        private static string GetCmdName(string command)
        {
            return command.EndsWith("_ack") ? command.Substring(0, command.Length - 4) : command;
        }

        public Task<IDictionary<string, object>> InvokeAsync(string command, string sid, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(new Dictionary<string, object> {{"cmd", command}, {"sid", sid}}, cancellationToken);
        }

        private void OnDataAvailable(byte[] buffer, int size)
        {
            if(!TryParseResponse(buffer, size, out var id, out var response)) return;

            if(!completions.TryRemove(id, out var completionSource)) return;

            completionSource.TrySetResult(response);
        }

        private async Task DispatchAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[ReceiveBufferSize];

            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var vt = socket.ReceiveAsync(buffer, None, cancellationToken);

                    var size = vt.IsCompletedSuccessfully ? vt.Result : await vt.AsTask().ConfigureAwait(false);

                    OnDataAvailable(buffer, size);
                }
                catch(OperationCanceledException)
                {
                    Trace.TraceInformation("Cancelling message dispatching loop...");
                }
                catch(Exception e)
                {
                    Trace.TraceError($"Error in message dispatch: {e.Message}");
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