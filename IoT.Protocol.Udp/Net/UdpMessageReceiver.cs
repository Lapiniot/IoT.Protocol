using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Udp.Net
{
    public class UdpMessageReceiver : UdpMessageReceiverBase
    {
        protected readonly IPEndPoint Endpoint;

        public UdpMessageReceiver(IPEndPoint endpoint) : base(Sockets.Udp.Connected(endpoint)) => Endpoint = endpoint;

        #region Overrides of UdpMessageReceiverBase

        public override async Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            return (await Socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false), Endpoint);
        }

        #endregion
    }
}