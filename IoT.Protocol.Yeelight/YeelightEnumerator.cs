using System.Net.Sockets;
using System.Policies;
using IoT.Protocol.Upnp;

namespace IoT.Protocol.Yeelight
{
    public class YeelightEnumerator : SsdpSearchEnumerator
    {
        public YeelightEnumerator(IRepeatPolicy discoveryPolicy) :
            base("wifi_bulb", Factory.GetIPv4MulticastGroup(1982), discoveryPolicy) {}
    }
}