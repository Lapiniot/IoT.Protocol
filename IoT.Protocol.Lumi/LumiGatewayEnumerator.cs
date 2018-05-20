using System.Json;
using System.Net;
using IoT.Protocol.Udp;

namespace IoT.Protocol.Lumi
{
    public class LumiEnumerator : UdpMulticastEnumerator<(IPAddress Address, ushort Port, string Sid)>
    {
        private readonly byte[] whoisMessage;

        public LumiEnumerator() : base(IPAddress.Parse("224.0.0.50"), 4321) => whoisMessage = new JsonObject {{"cmd", "whois"}}.Serialize();

        protected override byte[] CreateBuffer()
        {
            return new byte[0x100];
        }

        protected override (IPAddress Address, ushort Port, string Sid) ParseResponse(byte[] buffer, int size, IPEndPoint remoteEp)
        {
            var j = JsonExtensions.Deserialize(buffer, 0, size);

            return j["cmd"] == "iam" ? (IPAddress.Parse(j["ip"]), ushort.Parse(j["port"]), j["sid"]) : (null, 0, null);
        }

        protected override byte[] GetDiscoveryDatagram()
        {
            return whoisMessage;
        }
    }
}