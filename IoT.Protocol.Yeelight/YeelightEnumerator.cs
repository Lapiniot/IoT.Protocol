using System.Net;
using IoT.Protocol.Upnp;

namespace IoT.Protocol.Yeelight
{
    public class YeelightEnumerator : SsdpEnumerator
    {
        public YeelightEnumerator() :
            base(new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1982), "wifi_bulb")
        {
        }
    }
}