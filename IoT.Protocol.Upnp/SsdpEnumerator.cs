using System;
using System.IO;
using System.Net;
using IoT.Protocol.Udp;
using static System.Text.Encoding;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp
{
    public class SsdpEnumerator : UdpMulticastEnumerator<SsdpReply>
    {
        private static readonly IPAddress Address = IPAddress.Parse("239.255.255.250");
        private readonly string searchTarget;

        public SsdpEnumerator(int port = 1900, string searchTarget = All) : base(Address, port)
        {
            if(string.IsNullOrEmpty(searchTarget)) throw new ArgumentException("Parameter couldn't be null or empty.", nameof(searchTarget));

            this.searchTarget = searchTarget;
        }

        protected override int MaxRequestSize { get; } = 0x100;

        protected override int MaxResponseSize { get; } = 0x400;

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
                    writer.WriteLine("HOST: {0}:{1}", Address, RemoteEndPoint.Port);
                    writer.WriteLine("MAN: \"ssdp:discover\"");
                    writer.WriteLine("MX: {0}", 2);
                    writer.WriteLine("ST: {0}", searchTarget);
                    writer.WriteLine();
                }

                return stream.ToArray();
            }
        }
    }
}