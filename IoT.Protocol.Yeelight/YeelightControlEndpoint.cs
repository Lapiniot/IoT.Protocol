using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Json;
using System.Net;
using System.Net.Pipes;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;
using static System.Net.Sockets.ProtocolType;
using static System.Net.Sockets.SocketFlags;

namespace IoT.Protocol.Yeelight
{
    public class YeelightControlEndpoint : BufferedDataProcessor,
        IObservable<JsonObject>, IConnectedEndpoint<JsonObject, JsonValue>
    {
        private const byte CR = 0x0d;
        private const byte LF = 0x0a;

        private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonValue>> completions =
            new ConcurrentDictionary<long, TaskCompletionSource<JsonValue>>();

        private readonly ObserversContainer<JsonObject> observers;
        private long counter;
        private Socket socket;

        public YeelightControlEndpoint(uint deviceId, IPEndPoint endpoint)
        {
            Endpoint = endpoint;
            DeviceId = deviceId;
            observers = new ObserversContainer<JsonObject>();
        }

        public uint DeviceId { get; }

        protected TimeSpan CommandTimeout { get; } = TimeSpan.FromSeconds(10);

        public IPEndPoint Endpoint { get; }

        public async Task<JsonValue> InvokeAsync(JsonObject message, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<JsonValue>(cancellationToken);

            var (id, datagram) = await CreateRequestAsync(message, cancellationToken).ConfigureAwait(false);

            try
            {
                completions.TryAdd(id, completionSource);

                var vt = socket.SendAsync(datagram, None, cancellationToken);
                if(!vt.IsCompletedSuccessfully) await vt.AsTask().ConfigureAwait(false);

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

        public IDisposable Subscribe(IObserver<JsonObject> observer)
        {
            return observers.Subscribe(observer);
        }

        protected Task<(long, byte[])> CreateRequestAsync(JsonObject message, CancellationToken cancellationToken)
        {
            var id = Interlocked.Increment(ref counter);

            message["id"] = id;

            using(var ms = new MemoryStream())
            {
                message.SerializeTo(ms);
                ms.WriteByte(CR);
                ms.WriteByte(LF);
                return Task.FromResult((id, ms.ToArray()));
            }
        }

        protected bool TryParseResponse(byte[] bytes, int size, out long id, out JsonValue response)
        {
            var message = JsonExtensions.Deserialize(bytes, 0, size);

            id = 0;

            response = null;

            if(message is JsonObject json)
            {
                if(json.TryGetValue("id", out var value) && value.JsonType == JsonType.Number)
                {
                    id = value;

                    response = json;

                    return true;
                }

                if(json.TryGetValue("method", out var m) && m == "props" &&
                   json.TryGetValue("params", out var j) && j is JsonObject props)
                {
                    observers.Notify(props);
                }
            }

            return false;
        }

        protected void OnDataAvailable(byte[] buffer, int size)
        {
            if(TryParseResponse(buffer, size, out var id, out var response))
            {
                if(completions.TryRemove(id, out var completionSource))
                {
                    completionSource.TrySetResult(response);
                }
            }
        }

        protected override ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            return socket.ReceiveAsync(buffer, None, cancellationToken);
        }

        protected override long Process(in ReadOnlySequence<byte> buffer)
        {
            var pos = buffer.PositionOf(CR);

            if(pos == null) return 0;

            var position = buffer.GetPosition(1, pos.Value);

            if(!buffer.TryGet(ref position, out var mem) || mem.Length <= 0 || mem.Span[0] != LF) return 0;

            var sequence = buffer.Slice(0, pos.Value);
            var bytes = sequence.ToArray();
            OnDataAvailable(bytes, bytes.Length);

            return sequence.Length + 2;
        }

        protected override async Task OnConnectAsync(CancellationToken cancellationToken)
        {
            socket = new Socket(Endpoint.AddressFamily, SocketType.Stream, Tcp);
            await socket.ConnectAsync(Endpoint).ConfigureAwait(false);

            await base.OnConnectAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override async Task OnDisconnectAsync()
        {
            await base.OnDisconnectAsync().ConfigureAwait(false);

            await socket.DisconnectAsync().ConfigureAwait(false);
        }
    }
}