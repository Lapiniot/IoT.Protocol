using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Net;

namespace IoT.Protocol.Net.Tcp
{
    public class TcpMessenger : INetMessenger<byte[], string>
    {
        private readonly TcpClient client;
        private readonly IPEndPoint endpoint;
        private readonly StreamReader reader;
        private readonly NetworkStream stream;

        public TcpMessenger(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
            
            try
            {
                client = new TcpClient();
                client.Connect(endpoint);
                stream = client.GetStream();
                reader = new StreamReader(stream, Encoding.UTF8, false, 2 * 1024, true);
            }
            catch
            {
                client?.Dispose();
                stream?.Dispose();
                reader?.Dispose();

                throw;
            }
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
}