using System;
using System.Common;
using System.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Net;
using IoT.Protocol.Net.Tcp;

namespace IoT.Protocol.Yeelight
{
    public class YeelightControlEndpoint : DispatchingEndpoint<JsonObject, JsonValue, JsonValue, JsonValue, long>,
        IObservable<JsonObject>
    {
        private readonly ObservableContainer<JsonObject> container;
        private long counter;

        public YeelightControlEndpoint(uint deviceId, Uri location) :
            base(new IPEndPoint(IPAddress.Parse(location.Host), location.Port))
        {
            DeviceId = deviceId;
            container = new ObservableContainer<JsonObject>();
        }

        public uint DeviceId { get; }

        protected override TimeSpan CommandTimeout { get; } = TimeSpan.FromSeconds(10);

        public IDisposable Subscribe(IObserver<JsonObject> observer)
        {
            return container.Subscribe(observer);
        }

        protected override Task<(long, JsonValue)> CreateRequestAsync(JsonObject message, CancellationToken cancellationToken)
        {
            var id = Interlocked.Increment(ref counter);

            message["id"] = id;

            return Task.FromResult((id, (JsonValue)message));
        }

        protected override bool TryParseResponse(IPEndPoint remoteEndPoint, JsonValue responseMessage, out long id, out JsonValue response)
        {
            id = 0;

            response = null;

            if(responseMessage is JsonObject json)
            {
                if(json.TryGetValue("id", out var value) && value.JsonType == JsonType.Number)
                {
                    id = value;

                    response = json;

                    return true;
                }

                if(json.TryGetValue("method", out var m) && m == "props" &&
                   json.TryGetValue("params", out var j) && j is JsonObject props)
                {
                    container.Notify(props);
                }
            }

            return false;
        }

        #region Overrides of DispatchingMessenger<JsonValue, JsonValue>

        protected override INetMessenger<JsonValue, JsonValue> CreateNetMessenger()
        {
            return new TcpJsonMessenger(Endpoint);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing)
            {
                container.Dispose();
            }
        }
    }
}