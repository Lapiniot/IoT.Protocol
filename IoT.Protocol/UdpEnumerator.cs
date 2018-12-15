﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol
{
    /// <summary>
    /// Base abstract class for IoT devices enumerator which uses network discovery via UDP
    /// </summary>
    /// <typeparam name="TThing">Type of the 'thing' discoverable by concrete implementations</typeparam>
    public abstract class UdpEnumerator<TThing> : IThingEnumerator<TThing>
    {
        private readonly CreateSocketFactory createSocket;
        protected readonly IPEndPoint ReceiveFromEndpoint;
        protected readonly IPEndPoint SendToEndpoint;
        protected bool DistinctEndPoint;

        protected UdpEnumerator(CreateSocketFactory createSocketFactory, IPEndPoint sendToEndpoint, IPEndPoint receiveFromEndpoint)
        {
            SendToEndpoint = sendToEndpoint;
            createSocket = createSocketFactory;
            ReceiveFromEndpoint = receiveFromEndpoint;
            DistinctEndPoint = true;
        }

        protected UdpEnumerator(CreateSocketFactory createSocketFactory, IPEndPoint sendToEndpoint) :
            this(createSocketFactory, sendToEndpoint, Sockets.EndPoint.Any) {}

        protected abstract int SendBufferSize { get; }
        protected abstract int ReceiveBufferSize { get; }

        public async Task DiscoverAsync(Action<TThing> discovered, CancellationToken cancellationToken)
        {
            if(discovered == null) throw new ArgumentNullException(nameof(discovered));

            var endPoints = new HashSet<IPEndPoint>(new IPEndPointComparer());

            using(var socket = createSocket())
            {
                socket.ReceiveBufferSize = ReceiveBufferSize;
                socket.SendBufferSize = SendBufferSize;

                var datagram = GetDiscoveryDatagram();

                if(datagram.Length > SendBufferSize)
                {
                    throw new InvalidOperationException(
                        $"Discovery datagram is larger than {nameof(SendBufferSize)} = {SendBufferSize} configured buffer size");
                }

                var _ = SendDiscoveryDatagramAsync(socket, SendToEndpoint, datagram, TimeSpan.FromSeconds(3), cancellationToken);

                var buffer = new byte[ReceiveBufferSize];

                while(!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var (size, remoteEndPoint) = await socket.ReceiveFromAsync(buffer, ReceiveFromEndpoint, cancellationToken).ConfigureAwait(false);

                        if(DistinctEndPoint && !endPoints.Add(remoteEndPoint)) continue;

                        var vt = CreateInstanceAsync(buffer, size, remoteEndPoint, cancellationToken);
                        var instance = vt.IsCompletedSuccessfully ? vt.Result : await vt.AsTask().ConfigureAwait(false);

                        if(instance != null)
                        {
                            discovered(instance);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private async Task SendDiscoveryDatagramAsync(Socket socket, IPEndPoint endpoint, byte[] datagram, TimeSpan interval, CancellationToken cancellationToken)
        {
            try
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    await socket.SendToAsync(datagram, endpoint, cancellationToken).ConfigureAwait(false);
                    await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
                }
            }
            catch(OperationCanceledException)
            {
                // ignored
            }
        }

        /// <summary>
        /// Factory method to create IoT device instance by parsing discovery response datagram bytes
        /// </summary>
        /// <param name="buffer">Buffer containing message</param>
        /// <param name="size">Size of the valid data in the buffer</param>
        /// <param name="remoteEp">Responder endpoint information</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Instance of type
        /// <typeparam name="TThing" />
        /// </returns>
        protected abstract ValueTask<TThing> CreateInstanceAsync(byte[] buffer, int size, IPEndPoint remoteEp, CancellationToken cancellationToken);

        /// <summary>
        /// Returns datagram bytes to be send over the network for discovery
        /// </summary>
        /// <returns>Raw datagram bytes</returns>
        protected abstract byte[] GetDiscoveryDatagram();

        public class IPEndPointComparer : IEqualityComparer<IPEndPoint>
        {
            #region Implementation of IEqualityComparer<in IPEndPoint>

            public bool Equals(IPEndPoint x, IPEndPoint y)
            {
                if(x == null && y == null) return true;
                if(x == null || y == null) return false;
                return (x.Address == null ? y.Address == null : x.Address.Equals(y.Address)) && x.Port == y.Port;
            }

            public int GetHashCode(IPEndPoint obj)
            {
                if(obj?.Address == null) return 0;
                return obj.Address.GetHashCode() ^ obj.Port.GetHashCode();
            }

            #endregion
        }
    }
}