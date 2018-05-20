using System.Net;
using IoT.Protocol.Net;

namespace IoT.Protocol.Udp
{
    public abstract class UdpMulticastEnumerator<TThing> : UdpEnumerator<TThing>
    {
        protected UdpMulticastEnumerator(IPAddress address, int port) : base(address, port, SocketHandlers.UdpMulticast.Auto)
        {
        }
    }
}