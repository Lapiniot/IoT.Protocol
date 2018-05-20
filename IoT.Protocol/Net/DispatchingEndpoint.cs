using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    public abstract class DispatchingEndpoint<TRequest, TResponse, TRequestMessage, TResponseMessage, TKey> :
        DispatchingListener<TRequestMessage, TResponseMessage>, IConnectedEndpoint<TRequest, TResponse>
    {
        private readonly ConcurrentDictionary<TKey, TaskCompletionSource<TResponse>> completions =
            new ConcurrentDictionary<TKey, TaskCompletionSource<TResponse>>();

        protected IPEndPoint Endpoint;

        protected DispatchingEndpoint(IPEndPoint endpoint) => Endpoint = endpoint;

        protected abstract TimeSpan CommandTimeout { get; }

        public virtual async Task<TResponse> InvokeAsync(TRequest message, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<TResponse>(cancellationToken);

            var (id, datagram) = await CreateRequestAsync(message, cancellationToken).ConfigureAwait(false);

            try
            {
                completions.TryAdd(id, completionSource);

                await SendAsync(datagram, cancellationToken).ConfigureAwait(false);

                using(var timeoutSource = new CancellationTokenSource(CommandTimeout))
                using(completionSource.Bind(cancellationToken, timeoutSource.Token))
                {
                    return await completionSource.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                completions.TryRemove(id, out _);
            }
        }

        protected abstract bool TryParseResponse(IPEndPoint remoteEndPoint, TResponseMessage responseMessage, out TKey id, out TResponse response);

        protected abstract Task<(TKey, TRequestMessage)> CreateRequestAsync(TRequest message, CancellationToken cancellationToken);

        protected override void OnDataAvailable(IPEndPoint remoteEndpoint, TResponseMessage bytes)
        {
            if(TryParseResponse(remoteEndpoint, bytes, out var id, out var response))
            {
                if(completions.TryRemove(id, out var completionSource))
                {
                    completionSource.TrySetResult(response);
                }
            }
        }
    }
}