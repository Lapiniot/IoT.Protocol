using System.Json;
using System.Net;
using IoT.Protocol.Udp;

namespace IoT.Protocol.Lumi
{
    public class LumiEnumerator : UdpBroadcastEnumerator<(IPAddress Address, ushort Port, string Sid)>
    {
        private readonly byte[] whoisMessage;

        public LumiEnumerator() : base(new IPEndPoint(IPAddress.Parse("224.0.0.50"), 4321))
        {
            whoisMessage = new JsonObject {{"cmd", "whois"}}.Serialize();
        }

        protected override (IPAddress Address, ushort Port, string Sid) ParseResponse(byte[] buffer, IPEndPoint remoteEp)
        {
            var j = JsonExtensions.Deserialize(buffer);

            return j["cmd"] == "iam" ? (IPAddress.Parse(j["ip"]), ushort.Parse(j["port"]), j["sid"]) : (null, 0, null);
        }

        protected override byte[] GetDiscoveryDatagram()
        {
            return whoisMessage;
        }
    }
}