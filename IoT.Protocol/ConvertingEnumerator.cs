using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    public abstract class ConvertingEnumerator<TThing1, TThing2> : IThingEnumerator<TThing2>
    {
        protected IThingEnumerator<TThing1> Enumerator;

        protected ConvertingEnumerator(IThingEnumerator<TThing1> enumerator)
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

        public abstract TThing2 Convert(TThing1 thing);
    }
}