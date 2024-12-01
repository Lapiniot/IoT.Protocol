using System.Net;
using System.Net.Sockets;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using OOs.Policies;
using static System.Text.Encoding;
using static OOs.Net.Sockets.SocketBuilderExtensions;

namespace IoT.Protocol.Upnp;

public class SsdpSearchEnumerator : UdpSearchEnumerator<SsdpReply>
{
    private readonly Action<Socket, IPEndPoint> configureSocket;
    private readonly ReadOnlyMemory<byte> searchDatagram;

    public SsdpSearchEnumerator(string searchTarget, [NotNull] IPEndPoint groupEndPoint, string userAgent, Action<Socket, IPEndPoint> configureSocket, IRepeatPolicy searchRepeatPolicy) :
        base(groupEndPoint, searchRepeatPolicy)
    {
        if (string.IsNullOrEmpty(searchTarget))
        {
            throw new ArgumentException("Parameter couldn't be null or empty.", nameof(searchTarget));
        }

        this.configureSocket = configureSocket;
        searchDatagram = CreateDiscoveryDatagram(GroupEndPoint, searchTarget, userAgent ?? GetUserAgentString());

        static string GetUserAgentString() => $"{nameof(SsdpSearchEnumerator)}/{typeof(SsdpSearchEnumerator).Assembly.GetName().Version} ({RuntimeInformation.OSDescription.TrimEnd()})";
    }

    public SsdpSearchEnumerator(string searchTarget, Action<Socket, IPEndPoint> configureSocket, IRepeatPolicy searchRepeatPolicy) :
         this(searchTarget, GetIPv4SSDPGroup(), null, configureSocket, searchRepeatPolicy)
    { }

    public SsdpSearchEnumerator(string searchTarget, IRepeatPolicy searchRepeatPolicy) :
         this(searchTarget, GetIPv4SSDPGroup(), null, null, searchRepeatPolicy)
    { }

    protected sealed override void ConfigureSocket([NotNull] Socket socket, out IPEndPoint receiveEndPoint)
    {
        socket.ReceiveBufferSize = 0x400;
        socket.SendBufferSize = searchDatagram.Length;
        configureSocket?.Invoke(socket, GroupEndPoint);
        receiveEndPoint = GroupEndPoint;
    }

    protected sealed override bool TryParseDatagram(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEndPoint, out SsdpReply thing)
    {
        var span = buffer.Span;
        if (!span.StartsWith("M-SEARCH"u8))
        {
            return SsdpReply.TryParse(span, out thing);
        }

        thing = null;
        return false;
    }

    protected override ReadOnlyMemory<byte> CreateDiscoveryDatagram() => searchDatagram;

    private static ReadOnlyMemory<byte> CreateDiscoveryDatagram(IPEndPoint groupEndPoint, string searchTarget, string userAgent)
    {
        Span<byte> buffer = stackalloc byte[64];
        int addressBytesWritten;
        if (groupEndPoint.Address.AddressFamily == AddressFamily.InterNetwork)
        {
            groupEndPoint.Address.TryFormat(buffer, out addressBytesWritten);
        }
        else
        {
            buffer[0] = (byte)'[';
            groupEndPoint.Address.TryFormat(buffer.Slice(1), out addressBytesWritten);
            buffer[addressBytesWritten + 1] = (byte)']';
            addressBytesWritten += 2;
        }

        buffer[addressBytesWritten] = (byte)':';
        groupEndPoint.Port.TryFormat(buffer.Slice(addressBytesWritten + 1), out var portBytesWritten, default, default);
        var hostBytesWritten = addressBytesWritten + portBytesWritten + 1;
        var hostBytes = buffer.Slice(0, hostBytesWritten);

        var bytes = new byte[80 + hostBytesWritten + searchTarget.Length + userAgent.Length];
        var span = bytes.AsSpan();
        "M-SEARCH * HTTP/1.1\r\nHOST: "u8.CopyTo(span);
        var pos = 27;
        hostBytes.CopyTo(span.Slice(pos));
        pos += hostBytesWritten;
        "\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: "u8.CopyTo(span.Slice(pos));
        pos += 35;
        pos += ASCII.GetBytes(searchTarget, span.Slice(pos));
        "\r\nUSER-AGENT: "u8.CopyTo(span.Slice(pos));
        pos += 14;
        pos += ASCII.GetBytes(userAgent, span.Slice(pos));
        "\r\n\r\n"u8.CopyTo(span.Slice(pos));
        return bytes;
    }
}