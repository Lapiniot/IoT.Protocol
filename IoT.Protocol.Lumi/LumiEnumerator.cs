using System.Net;
using System.Net.Sockets;
using OOs.Net.Sockets;
using OOs.Policies;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Sockets.AddressFamily;
using static OOs.Net.NetworkInterfaceExtensions;

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

    protected override ReadOnlyMemory<byte> CreateDiscoveryDatagram() => /*lang=json,strict*/ "{\"cmd\":\"whois\"}"u8.ToArray();
}