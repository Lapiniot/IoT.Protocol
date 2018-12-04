using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Sockets.SocketFlags;

namespace IoT.Protocol.Udp.Net
{
    public class UdpMessageReceiver : UdpMessageReceiverBase
    {
        protected readonly IPEndPoint Endpoint;

        public UdpMessageReceiver(IPEndPoint endpoint) : base(Sockets.Udp.Connected(endpoint))
        {
            Endpoint = endpoint;
        }

        #region Overrides of UdpMessageReceiverBase

        public override ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            return Socket.ReceiveAsync(buffer, None, cancellationToken);
        }

        #endregion
    }
}