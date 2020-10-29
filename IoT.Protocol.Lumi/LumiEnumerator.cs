using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Policies;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Policies;
using static System.Globalization.CultureInfo;

namespace IoT.Protocol.Lumi
{
    public class LumiEnumerator : UdpEnumerator<(IPAddress Address, ushort Port, string Sid)>
    {
        public LumiEnumerator(IRepeatPolicy discoveryPolicy) :
            base(CreateSocket, new IPEndPoint(new IPAddress(0x320000e0 /*224.0.0.50*/), 4321), true, discoveryPolicy) {}

        protected override int SendBufferSize { get; } = 0xF;

        protected override int ReceiveBufferSize { get; } = 0x100;

        private static Socket CreateSocket(IPEndPoint _)
        {
            return Factory.CreateIPv4UdpMulticastSender();
        }

        protected override ValueTask<(IPAddress Address, ushort Port, string Sid)> CreateInstanceAsync(Memory<byte> buffer, IPEndPoint remoteEp,
            CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(buffer.Span);

            if(!json.TryGetProperty("cmd", out var cmd) || cmd.GetString() != "iam") throw new InvalidDataException("Invalid discovery response message");
            var result = (Address: IPAddress.Parse(json.GetProperty("ip").GetString() ?? throw new InvalidDataException("Missing value for property 'ip'")),
                Port: ushort.Parse(json.GetProperty("port").GetString() ?? throw new InvalidDataException("Missing value for property 'port'"), InvariantCulture),
                Sid: json.GetProperty("sid").GetString());
            return new ValueTask<(IPAddress, ushort, string)>(result);
        }

        protected override int WriteDiscoveryDatagram(Span<byte> span)
        {
            // {"cmd":"whois"}

            span[0] = 0x7B;
            span[1] = 0x22;
            span[2] = 0x63;
            span[3] = 0x6D;
            span[4] = 0x64;
            span[5] = 0x22;
            span[6] = 0x3A;
            span[7] = 0x22;
            span[8] = 0x77;
            span[9] = 0x68;
            span[10] = 0x6F;
            span[11] = 0x69;
            span[12] = 0x73;
            span[13] = 0x22;
            span[14] = 0x7D;

            return 15;
        }
    }
}