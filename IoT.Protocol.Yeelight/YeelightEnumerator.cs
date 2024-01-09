using IoT.Protocol.Upnp;
using OOs.Policies;
using OOs.Net.Sockets;

namespace IoT.Protocol.Yeelight;

public class YeelightEnumerator(IRepeatPolicy discoveryPolicy) :
    SsdpSearchEnumerator("wifi_bulb", SocketBuilderExtensions.GetIPv4MulticastGroup(1982), null, discoveryPolicy)
{ }