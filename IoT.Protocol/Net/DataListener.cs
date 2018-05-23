using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    public abstract class DataListener : DataReceiver
    {
        private CancellationTokenSource cancellationTokenSource;

        private async Task DispatchAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[2048];

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
                    Trace.TraceError($"Error in mesaage dispatch: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Process response datagram bytes
        /// </summary>
        /// <param name="remoteEndpoint">Remote endpoint of the data sender</param>
        /// <param name="bytes">Raw datagram bytes</param>
        protected abstract void OnDataAvailable(IPEndPoint remoteEndpoint, byte[] bytes, int size);

        #region Overrides of MessageReceiver<TMessage>

        protected override void OnConnect()
        {
            cancellationTokenSource = new CancellationTokenSource();

            var token = cancellationTokenSource.Token;

            Task.Run(() => DispatchAsync(token), token);
        }

        protected override void OnClose()
        {
            cancellationTokenSource?.Cancel();

            cancellationTokenSource?.Dispose();

            cancellationTokenSource = null;
        }

        #endregion
    }
}