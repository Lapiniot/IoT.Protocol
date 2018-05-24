using System.Net;
using System.Net.Sockets;

namespace IoT.Protocol.Udp
{
    public abstract class UdpMulticastEnumerator<TThing> : UdpEnumerator<TThing>
    {
        protected UdpMulticastEnumerator(IPAddress address, int port) : base(address, port, SocketsFactory.UdpMulticast.Auto)
        {
        }
    }
}