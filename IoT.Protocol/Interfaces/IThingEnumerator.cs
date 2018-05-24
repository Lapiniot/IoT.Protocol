using System.Collections.Generic;
using System.Threading;

namespace IoT.Protocol.Interfaces
{
    public interface IThingEnumerator<out TThing>
    {
        IEnumerable<TThing> Enumerate(CancellationToken cancellationToken);
    }
}