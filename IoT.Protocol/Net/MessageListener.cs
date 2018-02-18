using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    public abstract class MessageListener<TMessage> : MessageReceiver<TMessage>
    {
        private CancellationTokenSource cancellationTokenSource;

        private async Task DispatchAsync(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await ReceiveAsync(cancellationToken);

                    OnDataAvailable(result.RemoteEP, result.Message);
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
        /// <param name="remoteEndpoint"></param>
        /// <param name="bytes">Raw datagram bytes</param>
        protected abstract void OnDataAvailable(IPEndPoint remoteEndpoint, TMessage bytes);

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