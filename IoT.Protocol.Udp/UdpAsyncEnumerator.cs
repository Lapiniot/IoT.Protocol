using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Udp
{
    /// <summary>
    /// Simple <see cref="System.Threading.AsyncEnumerator{T}" /> implementation which receives UDP datagram asynchronously
    /// via provided instance of <seealso cref="UdpClient" /> and converts received data into a
    /// <typeparamref name="TThing" /> instance using parser function provided
    /// </summary>
    /// <typeparam name="TThing">Type of the parsing result</typeparam>
    public sealed class UdpAsyncEnumerator<TThing> : AsyncEnumerator<TThing>
    {
        private readonly UdpClient client;
        private readonly Func<byte[], IPEndPoint, TThing> factory;

        /// <summary>
        /// Type initializer
        /// </summary>
        /// <param name="udpClient"><see cref="UdpClient" /> instance to read data from</param>
        /// <param name="parser">UDP datagrams parser implementation</param>
        /// <exception cref="ArgumentNullException">When <paramref name="udpClient" /> is null</exception>
        /// <exception cref="ArgumentNullException">When <paramref name="parser" /> is null</exception>
        public UdpAsyncEnumerator(UdpClient udpClient, Func<byte[], IPEndPoint, TThing> parser)
        {
            client = udpClient ?? throw new ArgumentNullException(nameof(udpClient));
            factory = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <summary>
        /// Disposes instance
        /// </summary>
        public override void Dispose()
        {
            client.Dispose();
        }

        /// <summary>
        /// Receives and parses next available UDP datagram
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Instance of <typeparamref name="TThing" /></returns>
        /// <exception cref="TaskCanceledException">
        /// On external cancellation requested via <paramref name="cancellationToken" />
        /// </exception>
        public override async Task<TThing> GetNextAsync(CancellationToken cancellationToken)
        {
            var result = await client.ReceiveAsync().WaitAndUnwrapAsync(cancellationToken).ConfigureAwait(false);

            return factory(result.Buffer, result.RemoteEndPoint);
        }
    }
}