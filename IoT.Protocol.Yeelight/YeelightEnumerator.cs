using System;
using System.Net.Sockets;
using IoT.Protocol.Upnp;

namespace IoT.Protocol.Yeelight
{
    public class YeelightEnumerator : SsdpSearchEnumerator
    {
        public YeelightEnumerator() : base(SocketFactory.GetIPv4MulticastGroup(1982), "wifi_bulb", TimeSpan.FromSeconds(3)) {}
    }
}