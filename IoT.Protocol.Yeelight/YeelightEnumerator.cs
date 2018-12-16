using System;
using IoT.Protocol.Upnp;

namespace IoT.Protocol.Yeelight
{
    public class YeelightEnumerator : SsdpEnumerator
    {
        public YeelightEnumerator() : base(1982, "wifi_bulb", TimeSpan.FromSeconds(3)) {}
    }
}