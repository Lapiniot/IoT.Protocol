using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Lumi
{
    public class LumiEventListener : ActivityObject, IObservable<JsonElement>
    {
        private const int MaxReceiveBufferSize = 2048;
        private readonly IPEndPoint endpoint;
        private readonly ObserversContainer<JsonElement> observers;
        private Socket socket;
        private CancellationTokenSource tokenSource;

        public LumiEventListener(IPEndPoint groupEndpoint)
        {
            endpoint = groupEndpoint;
            observers = new ObserversContainer<JsonElement>();
        }

        #region Implementation of IObservable<out JsonElement>

        public IDisposable Subscribe(IObserver<JsonElement> observer)
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
            var buffer = new byte[MaxReceiveBufferSize];

            while(!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await socket.ReceiveFromAsync(buffer, default, endpoint).WaitAsync(cancellationToken).ConfigureAwait(false);

                    var message = JsonSerializer.Deserialize<JsonElement>(buffer.AsSpan(0, result.ReceivedBytes));

                    observers.Notify(message);
                }
                catch(OperationCanceledException)
                {
                    Trace.TraceInformation("Cancelling message dispatching loop...");
                }
                catch(Exception e)
                {
                    Trace.TraceError($"Error in message dispatch: {e.Message}");
                    throw;
                }
            }
        }

        public Task ConnectAsync(in CancellationToken cancellationToken)
        {
            return StartActivityAsync(cancellationToken);
        }

        public Task DisconnectAsync()
        {
            return StopActivityAsync();
        }

        #region Overrides of ActivityObject

        protected override Task StartingAsync(CancellationToken cancellationToken)
        {
            socket = Factory.CreateUdpMulticastListener(endpoint);

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