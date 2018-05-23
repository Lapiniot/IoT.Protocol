using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    /// <summary>
    /// Abstract base class for message receivers
    /// </summary>
    public abstract class DataReceiver : ConnectedObject
    {
        protected abstract Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken);
    }
}