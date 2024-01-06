using System.Net;
using System.Net.Sockets;
using System.Policies;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static System.Net.Sockets.SocketBuilderExtensions;
using static System.Text.Encoding;
using static System.Net.NetworkInterfaceExtensions;

namespace IoT.Protocol.Upnp;

public class SsdpSearchEnumerator : UdpSearchEnumerator<SsdpReply>
{
    private readonly string host;
    private readonly string searchTarget;
    private readonly Action<Socket, IPEndPoint> configureSocket;
    private readonly string userAgent;
    private readonly int datagramSize;

    public SsdpSearchEnumerator(string searchTarget, [NotNull] IPEndPoint groupEndPoint, Action<Socket, IPEndPoint> configureSocket, IRepeatPolicy repeatPolicy) :
        base(groupEndPoint, repeatPolicy)
    {
        if (string.IsNullOrEmpty(searchTarget))
        {
            throw new ArgumentException("Parameter couldn't be null or empty.", nameof(searchTarget));
        }

        this.searchTarget = searchTarget;
        this.configureSocket = configureSocket;
        host = groupEndPoint.ToString();
        userAgent = $"USER-AGENT: {nameof(SsdpSearchEnumerator)}/{typeof(SsdpSearchEnumerator).Assembly.GetName().Version} ({RuntimeInformation.OSDescription.TrimEnd()})";
        datagramSize = 68 + host.Length + searchTarget.Length + userAgent.Length;
    }

    public SsdpSearchEnumerator(string searchTarget, Action<Socket, IPEndPoint> configureSocket, IRepeatPolicy discoveryPolicy) :
         this(searchTarget, GetIPv4SSDPGroup(), configureSocket, discoveryPolicy)
    { }

    public SsdpSearchEnumerator(string searchTarget, IRepeatPolicy discoveryPolicy) :
         this(searchTarget, GetIPv4SSDPGroup(), null, discoveryPolicy)
    { }

    protected sealed override void ConfigureSocket([NotNull] Socket socket, out IPEndPoint receiveEndPoint)
    {
        socket.ReceiveBufferSize = 0x400;
        socket.SendBufferSize = datagramSize;

        if (configureSocket is not null)
        {
            configureSocket(socket, GroupEndPoint);
        }
        else
        {
            socket.ConfigureMulticast(FindBestMulticastInterface().GetIndex(socket.AddressFamily));
        }

        receiveEndPoint = GroupEndPoint;
    }

    protected sealed override bool TryParseDatagram(ReadOnlyMemory<byte> buffer, IPEndPoint remoteEndPoint, out SsdpReply thing) =>
        SsdpReply.TryParse(buffer.Span, out thing);

    protected override ReadOnlyMemory<byte> CreateDiscoveryDatagram()
    {
        var bytes = new byte[datagramSize];
        var span = bytes.AsSpan();
        "M-SEARCH * HTTP/1.1\r\nHOST: "u8.CopyTo(span);
        span = span[27..];
        var count = ASCII.GetBytes(host, span);
        span = span[count..];
        "\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: "u8.CopyTo(span);
        span = span[35..];
        count = ASCII.GetBytes(searchTarget, span);
        span[count++] = (byte)'\r';
        span[count++] = (byte)'\n';
        span = span[count..];
        count = ASCII.GetBytes(userAgent, span);
        span[count++] = (byte)'\r';
        span[count++] = (byte)'\n';
        span[count++] = (byte)'\r';
        span[count++] = (byte)'\n';
        return bytes;
    }
}