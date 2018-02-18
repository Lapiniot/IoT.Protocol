using System.Json;
using System.Net;
using IoT.Protocol.Lumi.Interfaces;
using IoT.Protocol.Net.Udp;

namespace IoT.Protocol.Lumi
{
    public class LumiEventListener : UdpMessageListener
    {
        private readonly ILumiEventListener listener;

        public LumiEventListener(ILumiEventListener eventListener, IPEndPoint groupEndpoint) : base(groupEndpoint)
        {
            listener = eventListener;
        }

        protected override void OnDataAvailable(IPEndPoint remoteEndPoint, byte[] bytes)
        {
            var message = (JsonObject) JsonExtensions.Deserialize(bytes);

            if(message.TryGetValue("sid", out var sid) &&
               message.TryGetValue("data", out var v) &&
               JsonValue.Parse(v) is JsonObject data)
            {
                switch((string) message["cmd"])
                {
                    case "report":
                    {
                        listener.OnReportMessage(sid, data, message);
                        break;
                    }
                    case "heartbeat":
                    {
                        listener.OnHeartbeatMessage(sid, data, message);
                        break;
                    }
                }
            }
        }
    }
}