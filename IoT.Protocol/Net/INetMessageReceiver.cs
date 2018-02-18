using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    /// <summary>
    /// Provides a posibility to receive messages from remote endpoint
    /// </summary>
    /// <typeparam name="TResponseMessage"></typeparam>
    public interface INetMessageReceiver<TResponseMessage> : IDisposable
    {
        /// <summary>
        /// Receives message from the remote endpoint asynchronously
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Message of type <see cref="TResponseMessage" /> and remote endpoint info</returns>
        Task<(IPEndPoint RemoteEP, TResponseMessage Message)> ReceiveAsync(CancellationToken cancellationToken);
    }
}