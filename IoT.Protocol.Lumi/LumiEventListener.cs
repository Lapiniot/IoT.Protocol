using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Lumi
{
    public class LumiEventListener : ActivityObject, IObservable<IDictionary<string, object>>
    {
        private const int ReceiveBufferSize = 0x8000;
        private readonly IPEndPoint endpoint;
        private readonly ObserversContainer<IDictionary<string, object>> observers;
        private Socket socket;
        private CancellationTokenSource tokenSource;

        public LumiEventListener(IPEndPoint groupEndpoint)
        {
            endpoint = groupEndpoint;
            observers = new ObserversContainer<IDictionary<string, object>>();
        }

        #region Implementation of IObservable<out IDictionary<string,object>>

        public IDisposable Subscribe(IObserver<IDictionary<string, object>> observer)
        {
            return observers.Subscribe(observer);
        }

        #endregion

        #region Overrides of ActivityObject

        public override ValueTask DisposeAsync()
        {
            using(observers) {}

            return base.DisposeAsync();
        }

        #endregion

        private async Task DispatchAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[ReceiveBufferSize];

            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await socket.ReceiveFromAsync(buffer, default, endpoint).WaitAsync(cancellationToken).ConfigureAwait(false);

                    var message = JsonSerializer.Deserialize<IDictionary<string, object>>(buffer[..result.ReceivedBytes]);

                    observers.Notify(message);
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

        #region Overrides of ActivityObject

        protected override Task StartingAsync(CancellationToken cancellationToken)
        {
            socket = SocketFactory.CreateUdpMulticastListener(endpoint);

            tokenSource = new CancellationTokenSource();

            var token = tokenSource.Token;

            Task.Run(() => DispatchAsync(token), token);

            return Task.CompletedTask;
        }

        protected override Task StoppingAsync()
        {
            var source = tokenSource;

            if(source != null)
            {
                source.Cancel();
                source.Dispose();
            }

            tokenSource = null;

            socket.Close();

            return Task.CompletedTask;
        }

        #endregion
    }
}