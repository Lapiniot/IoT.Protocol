using System;
using System.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Udp.Net;
using ILumiObserver = System.IObserver<(string Command, string Sid, System.Json.JsonObject Data, System.Json.JsonObject Message)>;

namespace IoT.Protocol.Lumi
{
    public class LumiEventListener : DataListener, IObservable<(string Command, string Sid, JsonObject Data, JsonObject Message)>
    {
        private readonly IPEndPoint endpoint;
        private readonly ObserversContainer<(string, string, JsonObject, JsonObject)> observers;
        private UdpMulticastMessageReceiver receiver;

        public LumiEventListener(IPEndPoint groupEndpoint)
        {
            endpoint = groupEndpoint;
            observers = new ObserversContainer<(string, string, JsonObject, JsonObject)>();
        }

        public IDisposable Subscribe(ILumiObserver observer)
        {
            return observers.Subscribe(observer);
        }

        protected override void OnDataAvailable(IPEndPoint remoteEndPoint, byte[] buffer, int size)
        {
            var message = (JsonObject)JsonExtensions.Deserialize(buffer, 0, size);

            if(message.TryGetValue("sid", out var sid) && message.TryGetValue("cmd", out var cmd) &&
               message.TryGetValue("data", out var v) && JsonValue.Parse(v) is JsonObject data)
            {
                observers.Notify((cmd, sid, data, message));
            }
        }

        public override async Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
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