using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol.Udp.Net
{
    public abstract class UdpMessageReceiverBase : IMessageReceiver
    {
        private bool disposed;
        protected Socket Socket;

        protected UdpMessageReceiverBase(Socket socket)
        {
            Socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        public void Dispose()
        {
            if(!disposed)
            {
                Socket?.Dispose();
                Socket = null;
                disposed = true;
            }
        }

        public abstract ValueTask<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken);
    }
}