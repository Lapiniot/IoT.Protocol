using System;
using System.Diagnostics;
using System.IO;
using System.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Net;

namespace IoT.Protocol.Net.Tcp
{
    public class TcpJsonMessenger : INetMessenger<JsonValue, JsonValue>
    {
        private readonly TcpClient client;
        private readonly IPEndPoint endpoint;
        private readonly StreamReader reader;
        private readonly NetworkStream netStream;

        public TcpJsonMessenger(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;

            try
            {
                client = new TcpClient();
                client.Connect(endpoint);
                netStream = client.GetStream();
                reader = new StreamReader(netStream, Encoding.UTF8, false, 2 * 1024, true);
            }
            catch
            {
                client?.Dispose();
                netStream?.Dispose();
                reader?.Dispose();

                throw;
            }
        }

        #region INetMessenger<JsonValue, JsonValue>

        public async Task<(IPEndPoint RemoteEP, JsonValue Message)> ReceiveAsync(CancellationToken cancellationToken)
        {
            string line = await reader.ReadLineAsync().WaitAsync(cancellationToken).ConfigureAwait(false);

            return (endpoint, JsonValue.Parse(line));
        }

        public Task SendAsync(JsonValue message, CancellationToken cancellationToken)
        {
            return Task.Run(() => SendMessage(message)).WaitAsync(cancellationToken);
        }

        private void SendMessage(JsonValue message)
        {
            message.SerializeTo(netStream);
            netStream.WriteByte(0x0d);
            netStream.WriteByte(0x0a);
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            reader.Dispose();
            netStream.Dispose();
            client.Dispose();
        }

        #endregion
    }
}