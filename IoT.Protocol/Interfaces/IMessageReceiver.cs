using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Interfaces
{
    /// <summary>
    /// Provides a posibility to receive messages from remote endpoint
    /// </summary>
    public interface IMessageReceiver : IDisposable
    {
        /// <summary>
        /// Receives data from the remote endpoint asynchronously
        /// </summary>
        Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken);
    }
}