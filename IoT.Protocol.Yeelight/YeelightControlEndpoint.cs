using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Memory;
using System.Net;
using System.Net.Pipes;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;
using static System.Net.Sockets.SocketFlags;
using static System.TimeSpan;

namespace IoT.Protocol.Yeelight
{
    public class YeelightControlEndpoint : PipeProducerConsumer,
        IObservable<JsonElement>, IConnectedEndpoint<IDictionary<string, object>, JsonElement>
    {
        private readonly ConcurrentDictionary<long, TaskCompletionSource<JsonElement>> completions =
            new ConcurrentDictionary<long, TaskCompletionSource<JsonElement>>();

        private readonly ObserversContainer<JsonElement> observers;
        private long counter;
        private Socket socket;

        public YeelightControlEndpoint(uint deviceId, IPEndPoint endpoint)
        {
            Endpoint = endpoint;
            DeviceId = deviceId;
            observers = new ObserversContainer<JsonElement>();
        }

        public uint DeviceId { get; }

        protected TimeSpan CommandTimeout { get; } = FromSeconds(10);

        public IPEndPoint Endpoint { get; }

        #region Implementation of IControlEndpoint<in IDictionary<string,object>,IDictionary<string,object>>

        public async Task<JsonElement> InvokeAsync(IDictionary<string, object> message, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<JsonElement>(cancellationToken);

            var id = Interlocked.Increment(ref counter);

            message["id"] = id;

            ReadOnlyMemory<byte> datagram;

            //TODO: use buffer pooling to avoid memory allocations here
            await using(var ms = new MemoryStream())
            {
                await using(var writer = new Utf8JsonWriter(ms))
                {
                    JsonSerializer.Serialize(writer, message);
                }

                ms.WriteByte(SequenceExtensions.CR);
                ms.WriteByte(SequenceExtensions.LF);
                datagram = ms.ToArray();
            }


            try
            {
                completions.TryAdd(id, completionSource);

                var vt = socket.SendAsync(datagram, None, cancellationToken);
                if(!vt.IsCompletedSuccessfully) await vt.ConfigureAwait(false);

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

        #region Implementation of IObservable<out IDictionary<string,object>>

        public IDisposable Subscribe(IObserver<JsonElement> observer)
        {
            return observers.Subscribe(observer);
        }

        #endregion

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
                var message = JsonSerializer.Deserialize<JsonElement>(line.Span);

                if(message.TryGetProperty("id", out var id))
                {
                    if(completions.TryRemove(id.GetInt64(), out var completion))
                    {
                        completion.TrySetResult(message);
                    }
                }
                else if(message.TryGetProperty("method", out var m) && m.GetString() == "props" &&
                        message.TryGetProperty("params", out var p))
                {
                    observers.Notify(p);
                }
            }
            catch(Exception e)
            {
                Trace.TraceError($"Error processing response message: {e.Message}");
            }

            return consumed;
        }

        protected override async Task StartingAsync(CancellationToken cancellationToken)
        {
            socket = new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(Endpoint).ConfigureAwait(false);
            await base.StartingAsync(cancellationToken).ConfigureAwait(false);
        }

        protected override async Task StoppingAsync()
        {
            await base.StoppingAsync().ConfigureAwait(false);

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