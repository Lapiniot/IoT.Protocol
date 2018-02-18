using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    /// <summary>
    /// Provides a possibility to send messages to the remote endpoint
    /// </summary>
    /// <typeparam name="TRequestMessage">Request message type</typeparam>
    public interface INetMessageSender<in TRequestMessage> : IDisposable
    {
        /// <summary>
        /// Sends <paramref name="message" /> to the remote endpoint asynchronously
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task that can be awaited</returns>
        Task SendAsync(TRequestMessage message, CancellationToken cancellationToken);
    }
}