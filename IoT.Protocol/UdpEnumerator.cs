﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable 8425

namespace IoT.Protocol
{
    /// <summary>
    /// Base abstract class for IoT devices enumerator which uses network discovery via UDP
    /// </summary>
    /// <typeparam name="TThing">Type of the 'thing' discoverable by concrete implementations</typeparam>
    public abstract class UdpEnumerator<TThing> : IAsyncEnumerable<TThing>
    {
        private readonly CreateSocketFactory createSocket;
        private readonly bool distinctAddress;
        private readonly TimeSpan pollInterval;

        protected UdpEnumerator(CreateSocketFactory createSocketFactory, IPEndPoint groupEndpoint,
            bool distinctAddress, TimeSpan pollInterval)
        {
            createSocket = createSocketFactory;
            GroupEndpoint = groupEndpoint;
            this.distinctAddress = distinctAddress;
            this.pollInterval = pollInterval;
        }

        protected abstract int SendBufferSize { get; }
        protected abstract int ReceiveBufferSize { get; }
        public IPEndPoint GroupEndpoint { get; }

        /// <summary>
        /// Factory method to create IoT device instance by parsing discovery response datagram bytes
        /// </summary>
        /// <param name="buffer">Buffer containing message</param>
        /// <param name="remoteEp">Responder endpoint information</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Instance of type
        /// <typeparam name="TThing" />
        /// </returns>
        protected abstract ValueTask<TThing> CreateInstanceAsync(Memory<byte> buffer, IPEndPoint remoteEp, CancellationToken cancellationToken);

        /// <summary>
        /// Returns datagram bytes to be send over the network for discovery
        /// </summary>
        /// <returns>Raw datagram bytes</returns>
        protected abstract int WriteDiscoveryDatagram(Span<byte> span);

        #region Implementation of IAsyncEnumerable<out TThing>

        public async IAsyncEnumerator<TThing> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var addresses = new HashSet<IPAddress>(EqualityComparer<IPAddress>.Default);

            using var socket = createSocket();
            socket.ReceiveBufferSize = ReceiveBufferSize;
            socket.SendBufferSize = SendBufferSize;

            var datagram = new byte[SendBufferSize];
            var buffer = new byte[ReceiveBufferSize];

            var count = WriteDiscoveryDatagram(datagram);

            var _ = StartDiscoverySenderAsync(socket, GroupEndpoint, new ArraySegment<byte>(datagram, 0, count), pollInterval, cancellationToken);

            while(!cancellationToken.IsCancellationRequested)
            {
                TThing instance = default;

                try
                {
                    var result = await socket.ReceiveFromAsync(buffer, default, GroupEndpoint).WaitAsync(cancellationToken).ConfigureAwait(false);
                    if(distinctAddress && !addresses.Add(((IPEndPoint)result.RemoteEndPoint).Address)) continue;
                    var vt = CreateInstanceAsync(buffer.AsMemory(0, result.ReceivedBytes), (IPEndPoint)result.RemoteEndPoint, cancellationToken);
                    instance = vt.IsCompletedSuccessfully ? vt.Result : await vt.ConfigureAwait(false);
                }
                catch(OperationCanceledException)
                {
                    // ignored as expected cancellation signal
                }
                catch(InvalidDataException)
                {
                    // ignored as expected if received datagram has wrong format
                }

                if(instance != null) yield return instance;
            }
        }

        private static async Task StartDiscoverySenderAsync(Socket socket, EndPoint endpoint, ArraySegment<byte> datagram, TimeSpan interval, CancellationToken cancellationToken)
        {
            try
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    await socket.SendToAsync(datagram, default, endpoint).WaitAsync(cancellationToken).ConfigureAwait(false);
                    await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
                }
            }
            catch(OperationCanceledException)
            {
                // ignored
            }
        }

        #endregion
    }
}