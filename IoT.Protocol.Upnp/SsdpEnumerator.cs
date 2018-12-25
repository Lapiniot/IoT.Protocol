using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Sockets.Sockets.Udp.Multicast;
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
            base(Sender, new IPEndPoint(new IPAddress(0xfaffffef /* 239.255.255.250 */), port), false, pollInterval)
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
            using(var stream = new MemoryStream())
            {
                using(var writer = new StreamWriter(stream, ASCII, 2048, true) {NewLine = "\r\n"})
                {
                    writer.WriteLine("M-SEARCH * HTTP/1.1");
                    writer.WriteLine("HOST: 239.255.255.250:{0}", port);
                    writer.WriteLine("MAN: \"ssdp:discover\"");
                    writer.WriteLine("MX: {0}", 2);
                    writer.WriteLine("ST: {0}", searchTarget);
                    writer.WriteLine(userAgent);
                    writer.WriteLine();
                }

                return stream.ToArray();
            }
        }
    }
}