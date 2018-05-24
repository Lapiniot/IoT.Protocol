using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Interfaces
{
    /// <summary>
    /// Provides a possibility to send data to the remote endpoint
    /// </summary>
    public interface IMessageSender : IDisposable
    {
        /// <summary>
        /// Sends <paramref name="buffer" /> to the remote endpoint asynchronously
        /// </summary>
        /// <param name="buffer">Message buffer to send</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task that can be awaited</returns>
        Task SendAsync(byte[] buffer, CancellationToken cancellationToken);
    }
}