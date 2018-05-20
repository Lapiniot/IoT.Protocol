using System.Net.Sockets;
using static System.Net.Sockets.AddressFamily;
using static System.Net.Sockets.SocketOptionName;
using static System.Net.Sockets.SocketType;

namespace IoT.Protocol.Net
{
    public delegate Socket CreateSocketHandler();

    public static class SocketHandlers
    {
        public static Socket UdpBroadcast()
        {
            var socket = new Socket(InterNetwork, Dgram, ProtocolType.Udp);

            socket.SetSocketOption(SocketOptionLevel.Socket, Broadcast, 1);

            return socket;
        }

        public static class UdpMulticast
        {
            public static Socket Auto()
            {
                return new Socket(InterNetwork, Dgram, ProtocolType.Udp);
            }
        }
    }
}