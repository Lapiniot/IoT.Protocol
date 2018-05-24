using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    /// <summary>
    /// Abstract base class for message receivers
    /// </summary>
    public abstract class DataReceiver : ConnectedObject
    {
        public abstract Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken);
    }
}