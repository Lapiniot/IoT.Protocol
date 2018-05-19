﻿using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    public interface IThingEnumerator<out TThing>
    {
        IEnumerable<TThing> Enumerate(CancellationToken cancellationToken);

        TThing Discover(IPEndPoint endpont, CancellationToken cancellationToken);
    }
}