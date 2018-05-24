using System.Net;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol.Udp.Net
{
    /// <summary>
    /// Base abstract type for IoT device controlled via UDP datagram messaging with dispatching queue support.
    /// </summary>
    public abstract class UdpDispatchingEndpoint<TRequest, TResponse, TKey> :
        DispatchingEndpoint<TRequest, TResponse, TKey>
    {
        protected UdpDispatchingEndpoint(IPEndPoint endpoint) : base(endpoint)
        {
        }

        #region Overrides of DispatchingMessenger<byte[],byte[]>

        protected override INetMessenger CreateNetMessenger()
        {
            return new UdpMessenger(Endpoint);
        }

        #endregion
    }
}