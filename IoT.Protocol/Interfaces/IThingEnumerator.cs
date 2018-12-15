using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Interfaces
{
    public interface IThingEnumerator<out TThing>
    {
        Task DiscoverAsync(Action<TThing> discovered, CancellationToken cancellationToken);
    }
}