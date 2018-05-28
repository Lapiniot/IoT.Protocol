using System;
using System.IO;
using System.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol.Yeelight
{
    public class YeelightControlEndpoint : DispatchingEndpoint<JsonObject, JsonValue, long>,
        IObservable<JsonObject>
    {
        private readonly ObserversContainer<JsonObject> observers;
        private long counter;

        public YeelightControlEndpoint(uint deviceId, Uri location) :
            base(new IPEndPoint(IPAddress.Parse(location.Host), location.Port))
        {
            DeviceId = deviceId;
            observers = new ObserversContainer<JsonObject>();
        }

        public uint DeviceId { get; }

        protected override TimeSpan CommandTimeout { get; } = TimeSpan.FromSeconds(10);

        public IDisposable Subscribe(IObserver<JsonObject> observer)
        {
            return observers.Subscribe(observer);
        }

        protected override Task<(long, byte[])> CreateRequestAsync(JsonObject message, CancellationToken cancellationToken)
        {
            var id = Interlocked.Increment(ref counter);

            message["id"] = id;

            using(var ms = new MemoryStream())
            {
                message.SerializeTo(ms);
                ms.WriteByte(0x0d);
                ms.WriteByte(0x0a);
                return Task.FromResult((id, ms.ToArray()));
            }
        }

        protected override bool TryParseResponse(byte[] bytes, int size, IPEndPoint remoteEndPoint, out long id, out JsonValue response)
        {
            var message = JsonExtensions.Deserialize(bytes, 0, size);
            
            id = 0;

            response = null;

            if(message is JsonObject json)
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
                    observers.Notify(props);
                }
            }

            return false;
        }

        #region Overrides of DispatchingMessenger<JsonValue, JsonValue>

        protected override INetMessenger CreateNetMessenger()
        {
            return new TcpLineMessenger(Endpoint);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(disposing)
            {
                observers.Dispose();
            }
        }
    }
}