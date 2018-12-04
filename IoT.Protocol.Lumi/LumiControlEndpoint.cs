﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;
using static System.Net.Sockets.ProtocolType;
using static System.Net.Sockets.SocketFlags;
using static System.Net.Sockets.SocketType;

namespace IoT.Protocol.Lumi
{
    public sealed class LumiControlEndpoint : AsyncConnectedObject, IConnectedEndpoint<JsonObject, JsonObject>
    {
        private const int ReceiveBufferSize = 0x8000;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonObject>> completions =
            new ConcurrentDictionary<string, TaskCompletionSource<JsonObject>>();

        private readonly IPEndPoint endpoint;
        private Socket socket;
        private CancellationTokenSource tokenSource;

        public LumiControlEndpoint(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
        }

        private TimeSpan CommandTimeout { get; } = TimeSpan.FromSeconds(10);

        public async Task<JsonObject> InvokeAsync(JsonObject message, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<JsonObject>(cancellationToken);

            var (id, datagram) = (GetCommandKey(message["cmd"], message["sid"]), message.Serialize());

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

        private bool TryParseResponse(byte[] buffer, int size, out string id, out JsonObject response)
        {
            var json = JsonExtensions.Deserialize(buffer, 0, size);
            if(json is JsonObject jObject && jObject.TryGetValue("cmd", out var cmd) && jObject.TryGetValue("sid", out var sid))
            {
                id = GetCommandKey(GetCmdName(cmd), sid);
                response = jObject;
                return true;
            }

            id = null;
            response = null;
            return false;
        }

        private string GetCommandKey(string command, string sid)
        {
            return $"{command}.{sid}";
        }

        private string GetCmdName(string command)
        {
            return command.EndsWith("_ack") ? command.Substring(0, command.Length - 4) : command;
        }

        public Task<JsonObject> InvokeAsync(string command, string sid, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(new JsonObject {{"cmd", command}, {"sid", sid}}, cancellationToken);
        }

        private void OnDataAvailable(byte[] buffer, int size)
        {
            if(TryParseResponse(buffer, size, out var id, out var response))
            {
                if(completions.TryRemove(id, out var completionSource))
                {
                    completionSource.TrySetResult(response);
                }
            }
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

        protected override async Task OnConnectAsync(CancellationToken cancellationToken)
        {
            socket = new Socket(endpoint.AddressFamily, Dgram, Udp);

            await socket.ConnectAsync(endpoint).ConfigureAwait(false);

            tokenSource = new CancellationTokenSource();

            var token = tokenSource.Token;

            var _ = Task.Run(() => DispatchAsync(token), token);
        }

        protected override async Task OnDisconnectAsync()
        {
            var source = tokenSource;

            if(source != null)
            {
                source.Cancel();
                source.Dispose();
            }

            tokenSource = null;

            await socket.DisconnectAsync().ConfigureAwait(false);

            socket.Close();
        }
    }
}