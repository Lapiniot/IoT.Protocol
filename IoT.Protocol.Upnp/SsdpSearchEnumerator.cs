using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.RuntimeInformation;
using static System.Text.Encoding;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp
{
    public class SsdpSearchEnumerator : UdpEnumerator<SsdpReply>
    {
        private readonly string host;
        private readonly string searchTarget;
        private readonly string userAgent;

        protected SsdpSearchEnumerator(IPEndPoint groupEndpoint, CreateSocketFactory socketFactory, string searchTarget, TimeSpan pollInterval) :
            base(socketFactory, groupEndpoint, false, pollInterval)
        {
            if(string.IsNullOrEmpty(searchTarget)) throw new ArgumentException("Parameter couldn't be null or empty.", nameof(searchTarget));

            this.searchTarget = searchTarget;
            host = GroupEndpoint.ToString();
            userAgent = $"USER-AGENT: {nameof(SsdpSearchEnumerator)}/{typeof(SsdpSearchEnumerator).Assembly.GetName().Version} ({OSDescription.TrimEnd()})";
            SendBufferSize = 68 + host.Length + searchTarget.Length + userAgent.Length;
        }

        public SsdpSearchEnumerator(IPEndPoint groupEndpoint, string searchTarget, TimeSpan pollInterval) :
            this(groupEndpoint, SocketFactory.CreateIPv4UdpMulticastSender, searchTarget, pollInterval) {}

        public SsdpSearchEnumerator(TimeSpan pollInterval, string searchTarget = All) :
            this(SocketFactory.GetIPv4SsdpGroup(), searchTarget, pollInterval) {}

        public SsdpSearchEnumerator(string searchTarget = All) :
            this(TimeSpan.FromSeconds(30), searchTarget) {}

        protected override int SendBufferSize { get; }

        protected override int ReceiveBufferSize { get; } = 0x400;

        protected override ValueTask<SsdpReply> CreateInstanceAsync(Memory<byte> buffer, IPEndPoint remoteEp, CancellationToken cancellationToken)
        {
            return new ValueTask<SsdpReply>(SsdpReply.Parse(buffer.Span));
        }

        protected override int WriteDiscoveryDatagram(Span<byte> span)
        {
            var count = ASCII.GetBytes("M-SEARCH * HTTP/1.1\r\nHOST: ", span);
            count += ASCII.GetBytes(host, span.Slice(count));
            count += ASCII.GetBytes("\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: ", span.Slice(count));
            count += ASCII.GetBytes(searchTarget, span.Slice(count));
            span[count++] = 13;
            span[count++] = 10;
            count += ASCII.GetBytes(userAgent, span.Slice(count));
            span[count++] = 13;
            span[count++] = 10;
            span[count++] = 13;
            span[count++] = 10;
            return count;
        }
    }
}