using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    /// <summary>
    /// Abstract base class for message receivers
    /// </summary>
    public abstract class DataReceiver : ConnectedObject
    {
        public abstract Task<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);
    }
}