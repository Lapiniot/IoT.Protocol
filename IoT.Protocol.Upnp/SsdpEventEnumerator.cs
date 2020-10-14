using System;
using System.Net;
using static System.Net.Sockets.Factory;

namespace IoT.Protocol.Upnp
{
    public sealed class SsdpEventEnumerator : SsdpSearchEnumerator
    {
        public SsdpEventEnumerator(IRetryPolicy discoveryPolicy) :
            this("ssdp:all", GetIPv4SSDPGroup(), discoveryPolicy)
        {
        }

        public SsdpEventEnumerator(string searchTarget, IRetryPolicy discoveryPolicy) :
            this(searchTarget, GetIPv4SSDPGroup(), discoveryPolicy)
        {
        }

        public SsdpEventEnumerator(string searchTarget, IPEndPoint groupEndpoint, IRetryPolicy discoveryPolicy) :
            base(searchTarget, groupEndpoint, CreateUdpMulticastSender, discoveryPolicy)
        {
        }
    }
}