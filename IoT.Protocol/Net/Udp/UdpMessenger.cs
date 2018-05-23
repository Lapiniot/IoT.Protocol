using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net.Udp
{
    public sealed class UdpMessenger : UdpMessageReceiver, INetMessenger
    {
        public UdpMessenger(IPEndPoint endpoint) : base(endpoint)
        {
        }
        
        #region Implementation of INetMessenger

        public Task SendAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}