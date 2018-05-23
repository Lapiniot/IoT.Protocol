using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Net;

namespace IoT.Protocol.Net.Udp
{
    public abstract class UdpMessageReceiverBase : IMessageReceiver
    {
        protected Socket Socket;
        private bool disposed;

        protected UdpMessageReceiverBase(Socket socket) => Socket = socket ?? throw new ArgumentNullException(nameof(socket));

        public void Dispose()
        {
            if(!disposed)
            {
                Socket?.Dispose();
                Socket = null;
                disposed = true;
            }
        }

        public Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}