using System;
using System.Net.Sockets;
using IoT.Protocol.Upnp;

namespace IoT.Protocol.Yeelight
{
    public class YeelightEnumerator : SsdpSearchEnumerator
    {
        public YeelightEnumerator(IRetryPolicy discoveryPolicy) :
            base("wifi_bulb", Factory.GetIPv4MulticastGroup(1982), discoveryPolicy)
        {
        }
    }
}