using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.TimeSpan;

namespace IoT.Protocol.Lumi
{
    public class LumiEnumerator : UdpEnumerator<(IPAddress Address, ushort Port, string Sid)>
    {
        private readonly byte[] whoisMessage;

        public LumiEnumerator() : base(SocketFactory.CreateUdpIPv4MulticastSender, new IPEndPoint(new IPAddress(0x320000e0 /*224.0.0.50*/), 4321), true, FromMinutes(5))
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

        protected override ValueTask<(IPAddress Address, ushort Port, string Sid)> CreateInstanceAsync(byte[] buffer, int size, IPEndPoint remoteEp,
            CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(buffer.AsSpan(0, size));

            if(json.TryGetProperty("cmd", out var cmd) && cmd.GetString() == "iam")
            {
                var result = (Address: IPAddress.Parse(json.GetProperty("ip").GetString()), Port: ushort.Parse(json.GetProperty("port").GetString()), Sid: json.GetProperty("sid").GetString());
                return new ValueTask<(IPAddress, ushort, string)>(result);
            }

            throw new InvalidDataException("Invalid discovery response message");
        }

        protected override int WriteDiscoveryDatagram(Span<byte> span)
        {
            whoisMessage.CopyTo(span);
            return whoisMessage.Length;
        }
    }
}