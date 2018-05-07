using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using IoT.Protocol.Udp;

namespace IoT.Protocol.Upnp
{
    public class SsdpEnumerator : UdpBroadcastEnumerator<SsdpReply>
    {
        private readonly string searchTarget;

        public SsdpEnumerator(IPEndPoint groupEndpoint, string searchTarget = UpnpServices.All) :
            base(groupEndpoint)
        {
            if(string.IsNullOrEmpty(searchTarget))
            {
                throw new ArgumentException("Parameter couldn't be null or empty.", nameof(searchTarget));
            }

            this.searchTarget = searchTarget;
        }

        public SsdpEnumerator(string searchTarget = UpnpServices.All) :
            this(new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900), searchTarget)
        {
            this.searchTarget = searchTarget;
        }

        protected override SsdpReply ParseResponse(byte[] buffer, IPEndPoint remoteEp)
        {
            using(var stream = new MemoryStream(buffer))
            {
                using(var reader = new StreamReader(stream, Encoding.ASCII))
                {
                    if(reader.ReadLine() == "HTTP/1.1 200 OK")
                    {
                        var reply = new SsdpReply();

                        while(!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();

                            if(line != null)
                            {
                                var index = line.IndexOf(": ", StringComparison.Ordinal);

                                if(index > 0) reply.Add(line.Substring(0, index), line.Substring(index + 2));
                            }
                        }

                        return reply;
                    }

                    throw new ApplicationException("Not a SSDP success response");
                }
            }
        }

        protected override byte[] GetDiscoveryDatagram()
        {
            using(var stream = new MemoryStream())
            {
                using(var writer = new StreamWriter(stream, Encoding.ASCII, 2048) {NewLine = "\r\n"})
                {
                    writer.WriteLine("M-SEARCH * HTTP/1.1");
                    writer.WriteLine("HOST: {0}:{1}", GroupEndpoint.Address, GroupEndpoint.Port);
                    writer.WriteLine("MAN: \"ssdp:discover\"");
                    writer.WriteLine("MX: {0}", 2);
                    writer.WriteLine("ST: {0}", searchTarget);
                    writer.WriteLine();
                }

                return stream.ToArray();
            }
        }

        protected override UdpClient CreateUdpClient(bool enableBroadcast)
        {
            var client = base.CreateUdpClient(enableBroadcast);

            client.Ttl = 4;

            return client;
        }
    }
}