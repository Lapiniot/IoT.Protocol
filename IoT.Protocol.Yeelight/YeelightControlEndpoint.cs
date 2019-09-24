using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Json;
using System.Memory;
using System.Net;
using System.Net.Pipes;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;
using static System.Json.JsonType;
using static System.Net.Sockets.ProtocolType;
using static System.Net.Sockets.SocketFlags;
using static System.TimeSpan;

namespace IoT.Protocol.Yeelight
{
    public class YeelightControlEndpoint : PipeProducerConsumer,
        IObservable<JsonObject>, IConnectedEndpoint<JsonObject, JsonValue>
    {
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

        protected TimeSpan CommandTimeout { get; } = FromSeconds(10);

        public IPEndPoint Endpoint { get; }

        #region Implementation of IControlEndpoint<in JsonObject,JsonValue>

        public async Task<JsonValue> InvokeAsync(JsonObject message, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<JsonValue>(cancellationToken);

            var id = Interlocked.Increment(ref counter);

            message["id"] = id;

            ReadOnlyMemory<byte> datagram;

            //TODO: use buffer pooling to avoid memory allocations here
            using(var ms = new MemoryStream())
            {
                message.SerializeTo(ms);
                ms.WriteByte(SequenceExtensions.CR);
                ms.WriteByte(SequenceExtensions.LF);
                datagram = ms.ToArray();
            }

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

        #endregion

        #region Implementation of IObservable<out JsonObject>

        public IDisposable Subscribe(IObserver<JsonObject> observer)
        {
            return observers.Subscribe(observer);
        }

        #endregion

        #region Overrides of PipeProducerConsumer

        protected override ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            return socket.ReceiveAsync(buffer, None, cancellationToken);
        }

        protected override long Consume(in ReadOnlySequence<byte> buffer)
        {
            if(!buffer.TryGetLine(out var line)) return 0;

            var consumed = line.Length + 2;

            try
            {
                var message = JsonExtensions.Deserialize(line);

                if(!(message is JsonObject json)) return consumed;

                if(json.TryGetValue("id", out var id) && id.JsonType == Number)
                {
                    if(completions.TryRemove(id, out var completion))
                    {
                        completion.TrySetResult(json);
                    }
                }
                else if(json.TryGetValue("method", out var m) && m == "props" &&
                        json.TryGetValue("params", out var p) && p is JsonObject props)
                {
                    observers.Notify(props);
                }
            }
            catch(Exception e)
            {
                Trace.TraceError($"Error processing response message: {e.Message}");
            }

            return consumed;
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
            
            var tcs = new TaskCompletionSource<bool>();
            
            var args = new SocketAsyncEventArgs {UserToken = tcs};
            
            args.Completed += OnDisconnectAsyncCompleted;
            
            try
            {
                socket.DisconnectAsync(args);
                await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                args.Completed -= OnDisconnectAsyncCompleted;
            }
        }

        private void OnDisconnectAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            var completionSource = (TaskCompletionSource<bool>)e.UserToken;

            if(e.SocketError != SocketError.Success)
            {
                completionSource.SetException(new SocketException((int)e.SocketError));
            }
            else
            {
                completionSource.SetResult(true);
            }
        }

        #endregion
    }
}