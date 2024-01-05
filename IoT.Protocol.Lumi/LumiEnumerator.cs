using System.Net;
using System.Net.Sockets;
using System.Policies;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Sockets.AddressFamily;
using static System.Net.NetworkInterfaceExtensions;

namespace IoT.Protocol.Lumi;

public record struct LumiEndpoint(IPEndPoint EndPoint, string Sid);

public sealed class LumiEnumerator(IRepeatPolicy repeatPolicy) : UdpSearchEnumerator<LumiEndpoint>(new(new IPAddress(0x320000e0 /*224.0.0.50*/), 4321), repeatPolicy)
{
    protected override void ConfigureSocket([NotNull] Socket socket, out IPEndPoint receiveEndPoint)
    {
        socket.ConfigureMulticast(FindBestMulticastInterface().GetIndex(InterNetwork));
        socket.SendBufferSize = 0x0F;
        socket.ReceiveBufferSize = 0x100;
        receiveEndPoint = GroupEndPoint;
    }

    protected override bool TryParseDatagram(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEndPoint, out LumiEndpoint thing)
    {
        var json = JsonSerializer.Deserialize(buffer.Span, JsonContext.Default.JsonElement);

        if (json.TryGetProperty("cmd", out var cmdProp) && cmdProp.GetString() == "iam" &&
            json.TryGetProperty("ip", out var ipProp) && IPAddress.TryParse(ipProp.GetString(), out var address) &&
            json.TryGetProperty("port", out var portProp) && int.TryParse(portProp.GetString(), out var port) &&
            json.TryGetProperty("sid", out var sid))
        {
            thing = new LumiEndpoint(new(address, port), sid.GetString());
            return true;
        }

        thing = default;
        return false;
    }

    protected override void WriteDiscoveryDatagram(Span<byte> span, out int bytesWritten)
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

        bytesWritten = 15;
    }
}