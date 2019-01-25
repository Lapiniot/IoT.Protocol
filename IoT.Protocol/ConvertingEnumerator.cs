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

        public async IAsyncEnumerable<TThing2> EnumerateAsync(CancellationToken cancellationToken)
        {
            var set = new HashSet<TThing1>(comparer);

            await foreach(var thing in Enumerator.EnumerateAsync(cancellationToken).ConfigureAwait(false))
            {
                if(set.Add(thing)) yield return Convert(thing);
            }
        }

        protected abstract TThing2 Convert(TThing1 thing);
    }
}