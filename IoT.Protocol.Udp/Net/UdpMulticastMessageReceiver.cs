using System.Net;
using System.Net.Sockets;

namespace IoT.Protocol.Udp.Net
{
    public sealed class UdpMulticastMessageReceiver : UdpMessageReceiverBase
    {
        public UdpMulticastMessageReceiver(IPEndPoint endpoint) : base(CreateMulticastSocket(endpoint))
        {
        }

        private static Socket CreateMulticastSocket(IPEndPoint endpoint)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(endpoint.Address));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 64);

            return socket;
        }
    }
}