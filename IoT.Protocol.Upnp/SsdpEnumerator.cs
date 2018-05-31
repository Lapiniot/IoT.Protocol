using System;
using System.IO;
using System.Net;
using IoT.Protocol.Udp;
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

        public SsdpEnumerator(int port = 1900, string searchTarget = All) : base(Sender, new IPEndPoint(new IPAddress(0xfaffffef /* 239.255.255.250 */), port))
        {
            if(string.IsNullOrEmpty(searchTarget)) throw new ArgumentException("Parameter couldn't be null or empty.", nameof(searchTarget));

            this.port = port;
            this.searchTarget = searchTarget;
            var type = typeof(SsdpEnumerator);
            userAgent = $"USER-AGENT: {nameof(SsdpEnumerator)}/{type.Assembly.GetName().Version} ({OSDescription.TrimEnd()})";
        }

        protected override int SendBufferSize { get; } = 0x200;

        protected override int ReceiveBufferSize { get; } = 0x400;

        protected SsdpReply ParseResponse(Span<byte> buffer, IPEndPoint remoteEp)
        {
            return SsdpReply.Parse(buffer);
        }

        protected override SsdpReply ParseResponse(byte[] buffer, int size, IPEndPoint remoteEp)
        {
            return ParseResponse(new Span<byte>(buffer, 0, size), remoteEp);
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