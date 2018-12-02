using System;
using System.Collections.Generic;
using System.Threading;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol
{
    public static class ThingEnumeratorExtensions
    {
        public static IEnumerable<TThing> Enumerate<TThing>(this IThingEnumerator<TThing> enumerator, TimeSpan timeout)
        {
            using(var cts = new CancellationTokenSource(timeout))
            {
                foreach(var thing in enumerator.Enumerate(cts.Token))
                {
                    yield return thing;
                }
            }
        }

        public static IEnumerable<TThing> Enumerate<TThing>(this IThingEnumerator<TThing> enumerator, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using(var timeoutTokenSource = new CancellationTokenSource(timeout))
            using(var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancellationToken))
            {
                foreach(var thing in enumerator.Enumerate(cts.Token))
                {
                    yield return thing;
                }
            }
        }

        public static IEnumerable<TThing> Enumerate<TThing>(this IThingEnumerator<TThing> enumerator, int timeoutMilliseconds)
        {
            using(var cts = new CancellationTokenSource(timeoutMilliseconds))
            {
                foreach(var thing in enumerator.Enumerate(cts.Token))
                {
                    yield return thing;
                }
            }
        }

        public static IEnumerable<TThing> Enumerate<TThing>(this IThingEnumerator<TThing> enumerator, int timeoutMilliseconds,
            CancellationToken cancellationToken)
        {
            using(var timeoutTokenSource = new CancellationTokenSource(timeoutMilliseconds))
            using(var cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancellationToken))
            {
                foreach(var thing in enumerator.Enumerate(cts.Token))
                {
                    yield return thing;
                }
            }
        }
    }
}