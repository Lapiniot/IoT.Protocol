using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Udp.Net
{
    public sealed class UdpMulticastMessageReceiver : UdpMessageReceiverBase
    {
        private readonly IPEndPoint endpoint;

        public UdpMulticastMessageReceiver(IPEndPoint endpoint) : base(Sockets.Udp.Multicast.Listener(endpoint))
        {
            this.endpoint = endpoint;
        }

        #region Overrides of UdpMessageReceiverBase

        public override async ValueTask<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            return await Socket.ReceiveFromAsync(buffer, endpoint, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}