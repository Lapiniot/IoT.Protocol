﻿using System.Net;

namespace IoT.Protocol.Net.Udp
{
    /// <summary>
    /// Base abstract type for IoT device controlled via UDP datagram messaging with dispatching queue support.
    /// </summary>
    public abstract class UdpDispatchingEndpoint<TRequest, TResponse, TKey> :
        DispatchingEndpoint<TRequest, TResponse, byte[], byte[], TKey>
    {
        protected UdpDispatchingEndpoint(IPEndPoint endpoint) : base(endpoint)
        {
        }

        #region Overrides of DispatchingMessenger<byte[],byte[]>

        protected override INetMessenger<byte[], byte[]> CreateNetMessenger()
        {
            return new UdpMessenger(Endpoint);
        }

        #endregion
    }
}