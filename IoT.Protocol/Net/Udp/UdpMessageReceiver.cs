using System.Net;
using System.Net.Sockets;

namespace IoT.Protocol.Net.Udp
{
    public class UdpMessageReceiver : UdpMessageReceiverBase
    {
        public UdpMessageReceiver(IPEndPoint endpoint) : base(CreateClient(endpoint))
        {
        }

        private static UdpClient CreateClient(IPEndPoint endpoint)
        {
            var client = new UdpClient {EnableBroadcast = false};

            client.Connect(endpoint);

            return client;
        }
    }
}