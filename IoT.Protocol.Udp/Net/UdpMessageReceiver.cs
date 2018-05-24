using System.Net;
using System.Net.Sockets;

namespace IoT.Protocol.Udp.Net
{
    public class UdpMessageReceiver : UdpMessageReceiverBase
    {
        public UdpMessageReceiver(IPEndPoint endpoint) : base(CreateClient(endpoint))
        {
        }

        private static Socket CreateClient(IPEndPoint endpoint)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect(endpoint);

            return socket;
        }
    }
}