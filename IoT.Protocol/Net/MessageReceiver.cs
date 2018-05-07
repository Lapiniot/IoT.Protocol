using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    /// <summary>
    /// Abstract base class for message receivers
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageReceiver<TMessage> : ConnectedObject
    {
        protected abstract Task<(IPEndPoint RemoteEP, TMessage Message)> ReceiveAsync(CancellationToken cancellationToken);
    }
}