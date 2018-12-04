using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Interfaces
{
    /// <summary>
    /// Provides a possibility to receive messages from remote endpoint
    /// </summary>
    public interface IMessageReceiver : IDisposable
    {
        /// <summary>
        /// Receives data from the remote endpoint asynchronously
        /// </summary>
        ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);
    }
}