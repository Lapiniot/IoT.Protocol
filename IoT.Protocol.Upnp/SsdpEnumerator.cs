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
    public class SsdpEnumerator : UdpEnumerator<SsdpReply>
    {
        private readonly int port;
        private readonly string searchTarget;
        private readonly string userAgent;

        public SsdpEnumerator(int port, string searchTarget, TimeSpan pollInterval) :
            base(SocketFactory.CreateUdpIPv4MulticastSender, new IPEndPoint(new IPAddress(0xfaffffef /* 239.255.255.250 */), port), false, pollInterval)
        {
            if(string.IsNullOrEmpty(searchTarget)) throw new ArgumentException("Parameter couldn't be null or empty.", nameof(searchTarget));

            this.port = port;
            this.searchTarget = searchTarget;
            var type = typeof(SsdpEnumerator);
            userAgent = $"USER-AGENT: {nameof(SsdpEnumerator)}/{type.Assembly.GetName().Version} ({OSDescription.TrimEnd()})";
        }

        public SsdpEnumerator(TimeSpan pollInterval, string searchTarget = All) :
            this(1900, searchTarget, pollInterval) {}

        public SsdpEnumerator(string searchTarget = All) :
            this(1900, searchTarget, TimeSpan.FromSeconds(5)) {}

        protected override int SendBufferSize { get; } = 0x200;

        protected override int ReceiveBufferSize { get; } = 0x400;

        protected override ValueTask<SsdpReply> CreateInstanceAsync(byte[] buffer, int size, IPEndPoint remoteEp, CancellationToken cancellationToken)
        {
            return new ValueTask<SsdpReply>(SsdpReply.Parse(new Span<byte>(buffer, 0, size)));
        }

        protected override byte[] GetDiscoveryDatagram()
        {
            var portStr = port.ToString();
            byte[] buffer = new byte[84 + portStr.Length + searchTarget.Length + userAgent.Length];
            Span<byte> span = buffer;
            var count = ASCII.GetBytes("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:", span);
            count += ASCII.GetBytes(portStr, span.Slice(count));
            count += ASCII.GetBytes("\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: ", span.Slice(count));
            count += ASCII.GetBytes(searchTarget, span.Slice(count));
            span[count++] = 13; span[count++] = 10;
            count += ASCII.GetBytes(userAgent, span.Slice(count));
            span[count++] = 13; span[count++] = 10;
            span[count++] = 13; span[count++] = 10;
            return buffer;
        }
    }
}