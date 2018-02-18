using System.Net;
using System.Net.Sockets;
using static System.Net.Sockets.SocketOptionName;

namespace IoT.Protocol.Net.Udp
{
    public sealed class UdpBroadcastMessageReceiver : UdpMessageReceiverBase
    {
        public UdpBroadcastMessageReceiver(IPEndPoint endpoint) : base(CreateBroadcastClient(endpoint))
        {
        }

        private static UdpClient CreateBroadcastClient(IPEndPoint endpoint)
        {
            var client = new UdpClient(endpoint.Port);

            client.Client.SetSocketOption(SocketOptionLevel.Socket, ReuseAddress, true);

            client.JoinMulticastGroup(endpoint.Address, 64);

            return client;
        }
    }
}