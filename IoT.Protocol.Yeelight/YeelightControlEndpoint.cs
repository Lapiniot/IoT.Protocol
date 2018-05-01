﻿using System;
using System.IO;
using System.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Net;

namespace IoT.Protocol.Yeelight
{
    public class YeelightControlEndpoint : DispatchingEndpoint<JsonObject, JsonValue, byte[], string, long>
    {
        private long counter;

        public YeelightControlEndpoint(uint deviceId, Uri location) :
            base(new IPEndPoint(IPAddress.Parse(location.Host), location.Port))
        {
            DeviceId = deviceId;
        }

        public uint DeviceId { get; }

        protected override TimeSpan CommandTimeout { get; } = TimeSpan.FromSeconds(10);

        protected override Task<(long, byte[])> CreateRequestAsync(JsonObject message, CancellationToken cancellationToken)
        {
            var id = Interlocked.Increment(ref counter);

            message["id"] = id;

            using(var stream = new MemoryStream())
            {
                message.SerializeTo(stream);

                stream.WriteByte(0x0d);
                stream.WriteByte(0x0a);
                stream.Flush();

                return Task.FromResult((id, stream.ToArray()));
            }
        }

        protected override bool TryParseResponse(IPEndPoint remoteEndPoint, string responseMessage, out long id, out JsonValue response)
        {
            id = 0;

            response = null;

            if(JsonValue.Parse(responseMessage) is JsonObject json &&
               json.TryGetValue("id", out var value) && value.JsonType == JsonType.Number)
            {
                id = value;

                response = json;

                return true;
            }

            return false;
        }


        #region Overrides of DispatchingMessenger<byte[],string>

        protected override INetMessenger<byte[], string> CreateNetMessenger()
        {
            return new TcpMessenger(Endpoint);
        }

        public class TcpMessenger : INetMessenger<byte[], string>
        {
            private readonly TcpClient client;
            private readonly IPEndPoint endpoint;
            private readonly StreamReader reader;
            private readonly NetworkStream stream;

            public TcpMessenger(IPEndPoint endpoint)
            {
                this.endpoint = endpoint;
                client = new TcpClient();
                client.Connect(endpoint);
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8, false, 2 * 1024, true);
            }

            #region Implementation of IDisposable

            public async Task<(IPEndPoint RemoteEP, string Message)> ReceiveAsync(CancellationToken cancellationToken)
            {
                return (endpoint, await reader.ReadLineAsync().WaitAsync(cancellationToken).ConfigureAwait(false));
            }

            public Task SendAsync(byte[] message, CancellationToken cancellationToken)
            {
                return stream.WriteAsync(message, 0, message.Length, cancellationToken);
            }

            public void Dispose()
            {
                reader.Dispose();
                stream.Dispose();
                client.Dispose();
            }

            #endregion
        }

        #endregion
    }
}