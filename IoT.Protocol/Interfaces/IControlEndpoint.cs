using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Interfaces
{
    public interface IControlEndpoint<in TRequest, TResponse>
    {
        Task<TResponse> InvokeAsync(TRequest message, CancellationToken cancellationToken);
    }
}