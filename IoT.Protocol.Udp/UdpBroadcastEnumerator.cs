using System.Net;
using System.Net.Sockets;

namespace IoT.Protocol.Udp
{
    /// <summary>
    /// Base abstaract class for IoT devices enumerator which uses network discovery via UDP datagram broadcasting
    /// </summary>
    /// <typeparam name="TThing">Type of the 'thing' discoverable by concrete implementations</typeparam>
    public abstract class UdpBroadcastEnumerator<TThing> : UdpEnumerator<TThing>
    {
        /// <summary>
        /// Type initializer
        /// </summary>
        protected UdpBroadcastEnumerator(int port) : base(IPAddress.Broadcast, port, SocketsFactory.UdpBroadcast)
        {
        }
    }
}