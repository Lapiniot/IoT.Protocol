using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    public abstract class ConvertingEnumerator<TThing1, TThing2> : IAsyncEnumerable<TThing2>
    {
        private readonly IEqualityComparer<TThing1> comparer;
        private readonly IAsyncEnumerable<TThing1> enumerator;

        protected ConvertingEnumerator(IAsyncEnumerable<TThing1> enumerator, IEqualityComparer<TThing1> comparer)
        {
            this.enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        #region Implementation of IAsyncEnumerable<out TThing2>

        public async IAsyncEnumerator<TThing2> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var set = new HashSet<TThing1>(comparer);

#pragma warning disable CA1508 // Looks like bug in the analyzer code 
            await foreach(var thing in enumerator.WithCancellation(cancellationToken).ConfigureAwait(false))
#pragma warning restore CA1508 // Looks like bug in the analyzer code 
            {
                if(set.Add(thing)) yield return Convert(thing);
            }
        }

        #endregion

        protected abstract TThing2 Convert(TThing1 thing);
    }
}