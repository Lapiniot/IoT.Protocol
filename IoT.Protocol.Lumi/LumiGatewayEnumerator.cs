using System.Json;
using System.Net;
using System.Net.Sockets;

namespace IoT.Protocol.Lumi
{
    public class LumiEnumerator : UdpEnumerator<(IPAddress Address, ushort Port, string Sid)>
    {
        private readonly byte[] whoisMessage;

        public LumiEnumerator() : base(Sockets.Udp.Multicast.Sender, new IPEndPoint(new IPAddress(0x320000e0 /*224.0.0.50*/), 4321))
        {
            whoisMessage = new byte[]
            {
                0x7B, 0x22, 0x63, 0x6D,
                0x64, 0x22, 0x3A, 0x22,
                0x77, 0x68, 0x6F, 0x69,
                0x73, 0x22, 0x7D
            };
        }

        protected override int SendBufferSize { get; } = 0x10;

        protected override int ReceiveBufferSize { get; } = 0x100;

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