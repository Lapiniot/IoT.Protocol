using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using IoT.Protocol.Net;

namespace IoT.Protocol.Udp
{
    /// <summary>
    /// Base abstaract class for IoT devices enumerator which uses network discovery via UDP
    /// </summary>
    /// <typeparam name="TThing">Type of the 'thing' discoverable by concrete implementations</typeparam>
    public abstract class UdpEnumerator<TThing> : IThingEnumerator<TThing>
    {
        private readonly CreateSocketHandler handler;
        protected readonly IPEndPoint RemoteEndPoint;

        protected UdpEnumerator(IPAddress address, int port, CreateSocketHandler createSocketHandler)
        {
            RemoteEndPoint = new IPEndPoint(address, port);
            handler = createSocketHandler;
        }

        /// <summary>
        /// Enumerates network devices by sending discovery datagrams
        /// </summary>
        /// <param name="cancellationToken">Cancelation token</param>
        /// <returns>Enumerable sequence of IoT devices that responded to discovery message </returns>
        public IEnumerable<TThing> Enumerate(CancellationToken cancellationToken = default)
        {
            using(var socket = handler())
            {
                if(cancellationToken.IsCancellationRequested) yield break;

                socket.SendToAsync(GetDiscoveryDatagram(), RemoteEndPoint, cancellationToken).GetAwaiter().GetResult();

                if(cancellationToken.IsCancellationRequested) yield break;

                var buffer = CreateBuffer();

                while(!cancellationToken.IsCancellationRequested)
                {
                    TThing instance;

                    try
                    {
                        var result = socket.ReceiveFromAsync(buffer, RemoteEndPoint, cancellationToken).GetAwaiter().GetResult();

                        if(cancellationToken.IsCancellationRequested) yield break;

                        instance = ParseResponse(buffer, result.Size, result.RemoteEndPoint);
                    }
                    catch(OperationCanceledException)
                    {
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

        protected abstract byte[] CreateBuffer();

        /// <summary>
        /// Factory method to create IoT device instance by parsing discovery response datagram bytes
        /// </summary>
        /// <param name="buffer">Buffer containing discovery response bytes</param>
        /// <param name="size">Size of the valid data in the buffer (in bytes)</param>
        /// <param name="remoteEp">Responder's endpoint information</param>
        /// <returns>
        /// Instance of type
        /// <typeparam name="TThing"></typeparam>
        /// </returns>
        protected abstract TThing ParseResponse(byte[] buffer, int size, IPEndPoint remoteEp);

        /// <summary>
        /// Returns datagram bytes to be send over the network for discovery
        /// </summary>
        /// <returns>Raw datagram bytes</returns>
        protected abstract byte[] GetDiscoveryDatagram();
    }
}