using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol.Udp
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

        protected UdpEnumerator(CreateSocketFactory createSocketFactory, IPEndPoint sendToEndpoint, IPEndPoint receiveFromEndpoint)
        {
            SendToEndpoint = sendToEndpoint;
            createSocket = createSocketFactory;
            ReceiveFromEndpoint = receiveFromEndpoint;
        }

        protected UdpEnumerator(CreateSocketFactory createSocketFactory, IPEndPoint sendToEndpoint) :
            this(createSocketFactory, sendToEndpoint, Sockets.EndPoint.Any) {}

        protected abstract int SendBufferSize { get; }
        protected abstract int ReceiveBufferSize { get; }

        /// <summary>
        /// Enumerates network devices by sending discovery datagram
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Enumerable sequence of IoT devices that responded to discovery message </returns>
        public IEnumerable<TThing> Enumerate(CancellationToken cancellationToken = default)
        {
            using(var socket = createSocket())
            {
                socket.ReceiveBufferSize = ReceiveBufferSize;
                socket.SendBufferSize = SendBufferSize;

                if(cancellationToken.IsCancellationRequested) yield break;

                var datagram = GetDiscoveryDatagram();

                if(datagram.Length > SendBufferSize)
                {
                    throw new InvalidOperationException(
                        $"Discovery datagram is larger than {nameof(SendBufferSize)} = {SendBufferSize} configured buffer size");
                }

                socket.SendToAsync(datagram, SendToEndpoint, cancellationToken).GetAwaiter().GetResult();

                if(cancellationToken.IsCancellationRequested) yield break;

                var buffer = new byte[ReceiveBufferSize];

                while(!cancellationToken.IsCancellationRequested)
                {
                    TThing instance;

                    try
                    {
                        var result = socket.ReceiveFromAsync(buffer, ReceiveFromEndpoint, cancellationToken).GetAwaiter().GetResult();

                        if(cancellationToken.IsCancellationRequested) yield break;

                        instance = ParseResponse(buffer, result.Size, result.RemoteEndPoint);
                    }
                    catch(OperationCanceledException)
                    {
                        //socket.Shutdown(SocketShutdown.Both);
                        yield break;
                    }
                    catch(Exception exception)
                    {
                        Debug.Write($"Error creating device instance: {exception.Message}", "UDPDeviceEnumerator");

                        // devices returning invalid data should not break enumeration!
                        continue;
                    }

                    yield return instance;
                }
            }
        }

        /// <summary>
        /// Factory method to create IoT device instance by parsing discovery response datagram bytes
        /// </summary>
        /// <param name="buffer">Buffer containing message</param>
        /// <param name="size">Size of the valid data in the buffer</param>
        /// <param name="remoteEp">Responder endpoint information</param>
        /// <returns>
        /// Instance of type
        /// <typeparam name="TThing" />
        /// </returns>
        protected abstract TThing ParseResponse(byte[] buffer, int size, IPEndPoint remoteEp);

        /// <summary>
        /// Returns datagram bytes to be send over the network for discovery
        /// </summary>
        /// <returns>Raw datagram bytes</returns>
        protected abstract byte[] GetDiscoveryDatagram();
    }
}