using System;
using System.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Net;
using IoT.Protocol.Net.Udp;
using ILumiObserver = System.IObserver<(string Command, string Sid, System.Json.JsonObject Data, System.Json.JsonObject Message)>;

namespace IoT.Protocol.Lumi
{
    public class LumiEventListener : MessageListener<byte[]>, IObservable<(string Command, string Sid, JsonObject Data, JsonObject Message)>
    {
        private readonly ObserversContainer<(string, string, JsonObject, JsonObject)> observers;
        private readonly IPEndPoint endpoint;
        private UdpMessageReceiver receiver;

        public LumiEventListener(IPEndPoint groupEndpoint)
        {
            endpoint = groupEndpoint;
            observers = new ObserversContainer<(string, string, JsonObject, JsonObject)>();
        }

        public IDisposable Subscribe(ILumiObserver observer)
        {
            return observers.Subscribe(observer);
        }

        protected override void OnDataAvailable(IPEndPoint remoteEndPoint, byte[] bytes)
        {
            var message = (JsonObject)JsonExtensions.Deserialize(bytes);

            if(message.TryGetValue("sid", out var sid) && message.TryGetValue("cmd", out var cmd) &&
               message.TryGetValue("data", out var v) && JsonValue.Parse(v) is JsonObject data)
            {
                observers.Notify((cmd, sid, data, message));
            }
        }

        protected override Task<(IPEndPoint RemoteEP, byte[] Message)> ReceiveAsync(CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckConnected();

            return receiver.ReceiveAsync(cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing)
            {
                observers.Dispose();
            }
        }

        #region Overrides of MessageListener<byte[]>

        protected override void OnConnect()
        {
            receiver = new UdpMessageReceiver(endpoint);

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