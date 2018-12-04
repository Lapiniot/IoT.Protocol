using System;
using System.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Udp.Net;

namespace IoT.Protocol.Lumi
{
    public class LumiEventListener : DataListener, IObservable<JsonObject>
    {
        private readonly IPEndPoint endpoint;
        private readonly ObserversContainer<JsonObject> observers;
        private UdpMulticastMessageReceiver receiver;

        public LumiEventListener(IPEndPoint groupEndpoint)
        {
            endpoint = groupEndpoint;
            observers = new ObserversContainer<JsonObject>();
        }

        public IDisposable Subscribe(IObserver<JsonObject> observer)
        {
            return observers.Subscribe(observer);
        }

        protected override void OnDataAvailable(byte[] buffer, int size)
        {
            var message = (JsonObject)JsonExtensions.Deserialize(buffer, 0, size);

            observers.Notify(message);
        }

        public override async Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckConnected();

            var valueTask = receiver.ReceiveAsync(buffer, cancellationToken);

            return valueTask.IsCompleted ? valueTask.Result : await valueTask.ConfigureAwait(false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing)
            {
                observers.Dispose();
            }
        }

        #region Overrides of DataListener

        protected override void OnConnect()
        {
            receiver = new UdpMulticastMessageReceiver(endpoint);

            base.OnConnect();
        }

        protected override void OnClose()
        {
            base.OnClose();

            receiver.Dispose();
        }

        #endregion
    }
}