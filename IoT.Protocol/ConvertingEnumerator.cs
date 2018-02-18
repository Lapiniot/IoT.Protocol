using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    public abstract class ConvertingEnumerator<T1, T2> : IThingEnumeratorAsync<T2>
    {
        protected IThingEnumeratorAsync<T1> Enumerator;

        protected ConvertingEnumerator(IThingEnumeratorAsync<T1> enumerator)
        {
            Enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        }

        public IEnumerable<T2> Enumerate(CancellationToken cancellationToken = default)
        {
            return Enumerator.Enumerate(cancellationToken).Select(Convert);
        }

        public T2 Discover(IPEndPoint endpont, CancellationToken cancellationToken = default)
        {
            return Convert(Enumerator.Discover(endpont, cancellationToken));
        }

        public async Task<AsyncEnumerator<T2>> GetEnumeratorAsync(CancellationToken cancellationToken = default)
        {
            return (await Enumerator.GetEnumeratorAsync(cancellationToken)).Map(Convert);
        }

        public async Task<T2> DiscoverAsync(IPEndPoint endpont, CancellationToken cancellationToken = default)
        {
            return Convert(await Enumerator.DiscoverAsync(endpont, cancellationToken));
        }

        public abstract T2 Convert(T1 thing);
    }
}