using System.Policies;
using IoT.Protocol.Upnp;

namespace IoT.Protocol.Yeelight;

public class YeelightEnumerator : SsdpSearchEnumerator
{
    public YeelightEnumerator(IRepeatPolicy discoveryPolicy) :
        base("wifi_bulb", SocketBuilderExtensions.GetIPv4MulticastGroup(1982), null, discoveryPolicy)
    { }
}