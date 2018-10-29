using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol
{
    public abstract class DataListener : DataReceiver
    {
        protected int ReceiveBufferSize = 0x8000;
        private CancellationTokenSource tokenSource;

        private async Task DispatchAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[ReceiveBufferSize];

            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var (size, ep) = await ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

                    OnDataAvailable(ep, buffer, size);
                }
                catch(OperationCanceledException)
                {
                    Trace.TraceInformation("Cancelling message dispatching loop...");
                }
                catch(Exception e)
                {
                    Trace.TraceError($"Error in message dispatch: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Process response datagram bytes
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of the data sender</param>
        /// <param name="buffer">Raw datagram bytes</param>
        /// <param name="size">Size of the actually valid data written to the <paramref name="buffer" /></param>
        protected abstract void OnDataAvailable(IPEndPoint remoteEndpoint, byte[] buffer, int size);

        #region Overrides of MessageReceiver<TMessage>

        protected override void OnConnect()
        {
            tokenSource = new CancellationTokenSource();

            var token = tokenSource.Token;

            Task.Run(() => DispatchAsync(token), token);
        }

        protected override void OnClose()
        {
            var source = tokenSource;

            if(source != null)
            {
                source.Cancel();
                source.Dispose();
            }

            tokenSource = null;
        }

        #endregion
    }
}