using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net.Udp
{
    public abstract class UdpMessageReceiverBase : INetMessageReceiver<byte[]>
    {
        protected UdpClient Client;
        private bool disposed;

        protected UdpMessageReceiverBase(UdpClient client)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public void Dispose()
        {
            if(!disposed)
            {
                Client?.Dispose();
                Client = null;
                disposed = true;
            }
        }

        public async Task<(IPEndPoint RemoteEP, byte[] Message)> ReceiveAsync(CancellationToken cancellationToken)
        {
            var result = await Client.ReceiveAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

            return (result.RemoteEndPoint, result.Buffer);
        }
    }
}