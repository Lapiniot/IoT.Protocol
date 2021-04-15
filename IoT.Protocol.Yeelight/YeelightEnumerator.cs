using System.Policies;
using IoT.Protocol.Upnp;
using static System.Net.Sockets.SocketBuilderExtensions;

namespace IoT.Protocol.Yeelight
{
    public class YeelightEnumerator : SsdpSearchEnumerator
    {
        public YeelightEnumerator(IRepeatPolicy discoveryPolicy) :
            base("wifi_bulb", GetIPv4MulticastGroup(1982), discoveryPolicy)
        { }
    }
}