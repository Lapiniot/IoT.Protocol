using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol.Udp.Net
{
    public sealed class UdpMessenger : UdpMessageReceiver, INetMessenger
    {
        public UdpMessenger(IPEndPoint endpoint) : base(endpoint) {}

        #region Implementation of INetMessenger

        public Task SendAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            return Socket.SendAsync(buffer, cancellationToken);
        }

        #endregion
    }
}