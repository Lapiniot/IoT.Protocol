using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    public interface IThingEnumeratorAsync<TThing> : IThingEnumerator<TThing>
    {
        Task<AsyncEnumerator<TThing>> GetEnumeratorAsync(CancellationToken cancellationToken);

        Task<TThing> DiscoverAsync(IPEndPoint endpont, CancellationToken cancellationToken);
    }
}