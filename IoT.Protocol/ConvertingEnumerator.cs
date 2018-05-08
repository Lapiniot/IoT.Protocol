using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    public abstract class ConvertingEnumerator<TThing1, TThing2> : IThingEnumeratorAsync<TThing2>
    {
        protected IThingEnumeratorAsync<TThing1> Enumerator;

        protected ConvertingEnumerator(IThingEnumeratorAsync<TThing1> enumerator)
        {
            Enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        }

        public IEnumerable<TThing2> Enumerate(CancellationToken cancellationToken = default)
        {
            return Enumerator.Enumerate(cancellationToken).Select(Convert);
        }

        public TThing2 Discover(IPEndPoint endpont, CancellationToken cancellationToken = default)
        {
            return Convert(Enumerator.Discover(endpont, cancellationToken));
        }

        public async Task<AsyncEnumerator<TThing2>> GetEnumeratorAsync(CancellationToken cancellationToken = default)
        {
            return (await Enumerator.GetEnumeratorAsync(cancellationToken).ConfigureAwait(false)).Map(Convert);
        }

        public async Task<TThing2> DiscoverAsync(IPEndPoint endpont, CancellationToken cancellationToken = default)
        {
            return Convert(await Enumerator.DiscoverAsync(endpont, cancellationToken).ConfigureAwait(false));
        }

        public abstract TThing2 Convert(TThing1 thing);
    }
}