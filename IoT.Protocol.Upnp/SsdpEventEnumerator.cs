using System;
using System.Net;
using static System.Net.Sockets.Factory;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp
{
    public sealed class SsdpEventEnumerator : SsdpSearchEnumerator
    {
        public SsdpEventEnumerator(IPEndPoint groupEndpoint, string searchTarget, TimeSpan pollInterval) :
            base(groupEndpoint, CreateUdpMulticastSender, searchTarget, pollInterval) {}

        public SsdpEventEnumerator(TimeSpan pollInterval, string searchTarget) : this(GetIPv4SSDPGroup(), searchTarget, pollInterval) {}

        public SsdpEventEnumerator(string searchTarget = All) : this(TimeSpan.FromSeconds(30), searchTarget) {}
    }
}