using System;
using System.Diagnostics;
using System.Json;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Lumi
{
    public class LumiEventListener : AsyncConnectedObject, IObservable<JsonObject>
    {
        private const int ReceiveBufferSize = 0x8000;
        private readonly IPEndPoint endpoint;
        private readonly ObserversContainer<JsonObject> observers;
        private Socket socket;
        private CancellationTokenSource tokenSource;

        public LumiEventListener(IPEndPoint groupEndpoint)
        {
            endpoint = groupEndpoint;
            observers = new ObserversContainer<JsonObject>();
        }

        public IDisposable Subscribe(IObserver<JsonObject> observer)
        {
            return observers.Subscribe(observer);
        }

        protected override Task OnConnectAsync(CancellationToken cancellationToken)
        {
            socket = Sockets.Udp.Multicast.Listener(endpoint);

            tokenSource = new CancellationTokenSource();

            var token = tokenSource.Token;

            Task.Run(() => DispatchAsync(token), token);

            return Task.CompletedTask;
        }

        protected override Task OnDisconnectAsync()
        {
            var source = tokenSource;

            if(source != null)
            {
                source.Cancel();
                source.Dispose();
            }

            tokenSource = null;

            socket.Close();

            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing) observers.Dispose();
        }

        private async Task DispatchAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[ReceiveBufferSize];

            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await socket.ReceiveFromAsync(buffer, default, endpoint).WaitAsync(cancellationToken).ConfigureAwait(false);

                    var message = (JsonObject)JsonExtensions.Deserialize(buffer, 0, result.ReceivedBytes);

                    observers.Notify(message);
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
    }
}