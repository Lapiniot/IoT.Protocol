using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Interfaces
{
    public interface IThingEnumerator<out TThing>
    {
        IAsyncEnumerable<TThing> EnumerateAsync(CancellationToken cancellationToken);
        Task DiscoverAsync<TState>(Func<TThing, TState, Task> discovered, TState state, CancellationToken cancellationToken);
    }
}