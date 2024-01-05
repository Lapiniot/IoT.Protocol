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
    private readonly int sendBufferSize;

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
        sendBufferSize = 68 + host.Length + searchTarget.Length + userAgent.Length;
    }

    public SsdpSearchEnumerator(string searchTarget, Action<Socket, IPEndPoint> configureSocket, IRepeatPolicy discoveryPolicy) :
         this(searchTarget, GetIPv4SSDPGroup(), configureSocket, discoveryPolicy)
    { }

    public SsdpSearchEnumerator(string searchTarget, IRepeatPolicy discoveryPolicy) :
         this(searchTarget, GetIPv4SSDPGroup(), null, discoveryPolicy)
    { }

    protected sealed override void WriteDiscoveryDatagram(Span<byte> span, out int bytesWritten)
    {
        var count = ASCII.GetBytes("M-SEARCH * HTTP/1.1\r\nHOST: ", span);
        count += ASCII.GetBytes(host, span[count..]);
        count += ASCII.GetBytes("\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: ", span[count..]);
        count += ASCII.GetBytes(searchTarget, span[count..]);
        span[count++] = 13;
        span[count++] = 10;
        count += ASCII.GetBytes(userAgent, span[count..]);
        span[count++] = 13;
        span[count++] = 10;
        span[count++] = 13;
        span[count++] = 10;
        bytesWritten = count;
    }

    protected sealed override void ConfigureSocket([NotNull] Socket socket, out IPEndPoint receiveEndPoint)
    {
        socket.ReceiveBufferSize = 0x400;
        socket.SendBufferSize = sendBufferSize;

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
}