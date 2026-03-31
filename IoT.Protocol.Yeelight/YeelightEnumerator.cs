using IoT.Protocol.Upnp;
using OOs.Net.Sockets;
using OOs.Resilience;

namespace IoT.Protocol.Yeelight;

public class YeelightEnumerator(IRepeatPolicy discoveryPolicy) :
    SsdpSearchEnumerator("wifi_bulb", SocketBuilderExtensions.GetIPv4MulticastGroup(1982), null, null, discoveryPolicy)
{ }