using System.Collections.Generic;
using System.Threading;

namespace IoT.Protocol.Interfaces
{
    public interface IThingEnumerator<out TThing>
    {
        IAsyncEnumerable<TThing> EnumerateAsync(CancellationToken cancellationToken);
    }
}