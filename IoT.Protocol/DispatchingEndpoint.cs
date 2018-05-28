using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol
{
    public abstract class DispatchingEndpoint<TRequest, TResponse, TKey> :
        DispatchingListener, IConnectedEndpoint<TRequest, TResponse>
    {
        private readonly ConcurrentDictionary<TKey, TaskCompletionSource<TResponse>> completions =
            new ConcurrentDictionary<TKey, TaskCompletionSource<TResponse>>();

        protected IPEndPoint Endpoint;

        protected DispatchingEndpoint(IPEndPoint endpoint) => Endpoint = endpoint;

        protected abstract TimeSpan CommandTimeout { get; }

        public async Task<TResponse> InvokeAsync(TRequest message, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<TResponse>(cancellationToken);

            var (id, datagram) = await CreateRequestAsync(message, cancellationToken).ConfigureAwait(false);

            try
            {
                completions.TryAdd(id, completionSource);

                await SendAsync(datagram, 0, datagram.Length, cancellationToken).ConfigureAwait(false);

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

        protected abstract bool TryParseResponse(byte[] buffer, int size, IPEndPoint remoteEndPoint, out TKey id, out TResponse response);

        protected abstract Task<(TKey, byte[])> CreateRequestAsync(TRequest message, CancellationToken cancellationToken);

        protected override void OnDataAvailable(IPEndPoint remoteEndpoint, byte[] buffer, int size)
        {
            if(TryParseResponse(buffer, size, remoteEndpoint, out var id, out var response))
            {
                if(completions.TryRemove(id, out var completionSource))
                {
                    completionSource.TrySetResult(response);
                }
            }
        }
    }
}