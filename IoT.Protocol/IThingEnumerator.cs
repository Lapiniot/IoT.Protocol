using System.Collections.Generic;
using System.Threading;

namespace IoT.Protocol
{
    public interface IThingEnumerator<out TThing>
    {
        IEnumerable<TThing> Enumerate(CancellationToken cancellationToken);
    }
}