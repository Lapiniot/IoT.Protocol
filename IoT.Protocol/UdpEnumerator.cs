using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
        protected readonly IPEndPoint ReceiveFromEndpoint;
        protected readonly IPEndPoint SendToEndpoint;

        protected UdpEnumerator(CreateSocketFactory createSocketFactory, IPEndPoint sendToEndpoint,
            bool distinctAddress, TimeSpan pollInterval)
        {
            createSocket = createSocketFactory;
            SendToEndpoint = sendToEndpoint;
            ReceiveFromEndpoint = new IPEndPoint(IPAddress.Any, 0);
            this.distinctAddress = distinctAddress;
            this.pollInterval = pollInterval;
        }

        protected abstract int SendBufferSize { get; }
        protected abstract int ReceiveBufferSize { get; }

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

        #region Implementation of IAsyncEnumerable<out TThing>

        public async IAsyncEnumerator<TThing> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var addresses = new HashSet<IPAddress>(EqualityComparer<IPAddress>.Default);

            using var socket = createSocket();
            socket.ReceiveBufferSize = ReceiveBufferSize;
            socket.SendBufferSize = SendBufferSize;

            var datagram = GetDiscoveryDatagram();

            if(datagram.Length > SendBufferSize)
            {
                throw new InvalidOperationException(
                    $"Discovery datagram is larger than {nameof(SendBufferSize)} = {SendBufferSize} configured buffer size");
            }

            var _ = SendDiscoveryDatagramAsync(socket, SendToEndpoint, datagram, pollInterval, cancellationToken);

            var buffer = new byte[ReceiveBufferSize];

            var ep = new IPEndPoint(SendToEndpoint.Address, 0);

            while(!cancellationToken.IsCancellationRequested)
            {
                TThing instance = default;

                try
                {
                    var result = await socket.ReceiveFromAsync(buffer, default, ep).WaitAsync(cancellationToken).ConfigureAwait(false);

                    if(distinctAddress && !addresses.Add(((IPEndPoint)result.RemoteEndPoint).Address)) continue;

                    var vt = CreateInstanceAsync(buffer, result.ReceivedBytes, (IPEndPoint)result.RemoteEndPoint, cancellationToken);
                    instance = vt.IsCompletedSuccessfully ? vt.Result : await vt.AsTask().ConfigureAwait(false);
                }
                catch(OperationCanceledException)
                {
                    // ignored as expected
                }

                if(instance != null) yield return instance;
            }
        }

        private async Task SendDiscoveryDatagramAsync(Socket socket, EndPoint endpoint, byte[] datagram, TimeSpan interval, CancellationToken cancellationToken)
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