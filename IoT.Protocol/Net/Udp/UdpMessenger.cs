using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net.Udp
{
    public sealed class UdpMessenger : UdpMessageReceiver, INetMessenger<byte[], byte[]>
    {
        public UdpMessenger(IPEndPoint endpoint) : base(endpoint)
        {
        }

        #region Implementation of INetMessenger<in byte[],byte[]>

        public Task SendAsync(byte[] message, CancellationToken cancellationToken)
        {
            return Client.SendAsync(message, message.Length).WaitAsync(cancellationToken);
        }

        #endregion
    }
}