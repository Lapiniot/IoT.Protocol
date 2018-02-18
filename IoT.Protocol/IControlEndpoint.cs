using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    public interface IControlEndpoint<in TRequest, TResponse> : IDisposable
    {
        Task<TResponse> InvokeAsync(TRequest message, CancellationToken cancellationToken);

        void Connect();

        void Close();
    }
}