using System;
using System.Collections.Generic;
using System.Threading;

namespace IoT.Protocol
{
    public static class ThingEnumeratorExtensions
    {
        public static IEnumerable<TThing> Enumerate<TThing>(this IThingEnumerator<TThing> enumerator, TimeSpan timeout)
        {
            using(var cts = new CancellationTokenSource(timeout))
            {
                foreach(var thing in enumerator.Enumerate(cts.Token)) yield return thing;
            }
        }

        public static IEnumerable<TThing> Enumerate<TThing>(this IThingEnumerator<TThing> enumerator, int timeoutMilliseconds)
        {
            using(var cts = new CancellationTokenSource(timeoutMilliseconds))
            {
                foreach(var thing in enumerator.Enumerate(cts.Token)) yield return thing;
            }
        }
    }
}