﻿using System.Net;
using System.Net.Sockets;
using System.Policies;
using static System.Net.Sockets.SocketBuilderExtensions;
using static System.Runtime.InteropServices.RuntimeInformation;
using static System.Text.Encoding;
using static System.Net.NetworkInterfaceExtensions;

namespace IoT.Protocol.Upnp;

public class SsdpSearchEnumerator : UdpEnumerator<SsdpReply>
{
    private readonly string host;
    private readonly string searchTarget;
    private readonly string userAgent;

    public SsdpSearchEnumerator(string searchTarget, IPEndPoint groupEndpoint, IRepeatPolicy discoveryPolicy, CreateSocketFactory socketFactory) :
        base(socketFactory, groupEndpoint, false, discoveryPolicy)
    {
        if(string.IsNullOrEmpty(searchTarget)) throw new ArgumentException("Parameter couldn't be null or empty.", nameof(searchTarget));

        this.searchTarget = searchTarget;
        host = GroupEndpoint.ToString();
        userAgent = $"USER-AGENT: {nameof(SsdpSearchEnumerator)}/{typeof(SsdpSearchEnumerator).Assembly.GetName().Version} ({OSDescription.TrimEnd()})";
        SendBufferSize = 68 + host.Length + searchTarget.Length + userAgent.Length;
    }

    public SsdpSearchEnumerator(string searchTarget, IRepeatPolicy discoveryPolicy, CreateSocketFactory socketFactory) :
        this(searchTarget, GetIPv4SSDPGroup(), discoveryPolicy, socketFactory)
    { }

    public SsdpSearchEnumerator(string searchTarget, IPEndPoint groupEndpoint, IRepeatPolicy discoveryPolicy) :
        this(searchTarget, groupEndpoint, discoveryPolicy,
            ep => CreateUdp(ep.AddressFamily).ConfigureMulticast(
                FindBestMulticastInterface().GetIndex(ep.AddressFamily)))
    { }

    public SsdpSearchEnumerator(string searchTarget, IRepeatPolicy discoveryPolicy) :
        this(searchTarget, GetIPv4SSDPGroup(), discoveryPolicy,
            ep => CreateUdp(ep.AddressFamily).ConfigureMulticast(
                FindBestMulticastInterface().GetIndex(ep.AddressFamily)))
    { }

    protected override int SendBufferSize { get; }
    protected override int ReceiveBufferSize { get; } = 0x400;

    protected override ValueTask<SsdpReply> CreateInstanceAsync(Memory<byte> buffer, IPEndPoint remoteEp, CancellationToken cancellationToken)
    {
        return new(SsdpReply.Parse(buffer.Span));
    }

    protected override void WriteDiscoveryDatagram(Span<byte> span, out int bytesWritten)
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
}