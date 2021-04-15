using System.Net;
using System.Policies;
using static System.Net.Sockets.SocketBuilderExtensions;

namespace IoT.Protocol.Upnp
{
    public sealed class SsdpEventEnumerator : SsdpSearchEnumerator
    {
        public SsdpEventEnumerator(IRepeatPolicy discoveryPolicy) :
            this("ssdp:all", GetIPv4SSDPGroup(), discoveryPolicy)
        { }

        public SsdpEventEnumerator(string searchTarget, IRepeatPolicy discoveryPolicy) :
            this(searchTarget, GetIPv4SSDPGroup(), discoveryPolicy)
        { }

        public SsdpEventEnumerator(string searchTarget, IPEndPoint groupEndpoint, IRepeatPolicy discoveryPolicy) :
            base(searchTarget, groupEndpoint, group => CreateUdp().JoinMulticastGroup(group), discoveryPolicy)
        { }
    }
}