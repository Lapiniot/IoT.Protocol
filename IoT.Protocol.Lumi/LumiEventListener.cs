using System;
using System.Common;
using System.Json;
using System.Net;
using IoT.Protocol.Net.Udp;
using ILumiObserver = System.IObserver<(string Command, string Sid, System.Json.JsonObject Data, System.Json.JsonObject Message)>;

namespace IoT.Protocol.Lumi
{
    public class LumiEventListener : UdpMessageListener, IObservable<(string Command, string Sid, JsonObject Data, JsonObject Message)>
    {
        private readonly ObservableContainer<(string, string, JsonObject, JsonObject)> container;

        public LumiEventListener(IPEndPoint groupEndpoint) : base(groupEndpoint)
        {
            container = new ObservableContainer<(string, string, JsonObject, JsonObject)>();
        }

        public IDisposable Subscribe(ILumiObserver observer)
        {
            return container.Subscribe(observer);
        }

        protected override void OnDataAvailable(IPEndPoint remoteEndPoint, byte[] bytes)
        {
            var message = (JsonObject)JsonExtensions.Deserialize(bytes);

            if(message.TryGetValue("sid", out var sid) && message.TryGetValue("cmd", out var cmd) &&
               message.TryGetValue("data", out var v) && JsonValue.Parse(v) is JsonObject data)
            {
                container.Notify((cmd, sid, data, message));
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing) container.Dispose();
        }
    }
}