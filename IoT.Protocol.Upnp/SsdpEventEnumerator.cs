using System;
using System.Net;
using System.Net.Sockets;

namespace IoT.Protocol.Upnp
{
    public sealed class SsdpEventEnumerator : SsdpSearchEnumerator
    {
        public SsdpEventEnumerator(IPEndPoint groupEndpoint, string searchTarget, TimeSpan pollInterval) :
            base(groupEndpoint, () => SocketFactory.CreateIPv4UdpMulticastSender(groupEndpoint), searchTarget, pollInterval) {}

        public SsdpEventEnumerator(TimeSpan pollInterval, string searchTarget = "ssdp:all") :
            this(SocketFactory.GetIPv4SsdpGroup(), searchTarget, pollInterval) {}

        public SsdpEventEnumerator(string searchTarget = "ssdp:all") :
            this(SocketFactory.GetIPv4SsdpGroup(), searchTarget, TimeSpan.FromSeconds(30)) {}
    }
}