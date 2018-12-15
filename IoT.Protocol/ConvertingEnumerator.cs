using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol
{
    public abstract class ConvertingEnumerator<TThing1, TThing2> : IThingEnumerator<TThing2>
    {
        private readonly IEqualityComparer<TThing1> comparer;
        protected IThingEnumerator<TThing1> Enumerator;

        protected ConvertingEnumerator(IThingEnumerator<TThing1> enumerator, IEqualityComparer<TThing1> comparer)
        {
            Enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public Task DiscoverAsync(Action<TThing2> discovered, CancellationToken cancellationToken)
        {
            var set = new HashSet<TThing1>(comparer);

            void DiscoveredAdapter(TThing1 thing)
            {
                if(set.Add(thing))
                {
                    discovered(Convert(thing));
                }
            }

            return Enumerator.DiscoverAsync(DiscoveredAdapter, cancellationToken);
        }

        protected abstract TThing2 Convert(TThing1 thing);
    }
}