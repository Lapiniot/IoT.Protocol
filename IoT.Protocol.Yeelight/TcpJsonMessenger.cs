using System;
using System.IO;
using System.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol.Yeelight
{
    public class TcpJsonMessenger : INetMessenger
    {
        private TcpClient client;
        private NetworkStream netStream;
        private StreamReader reader;

        public TcpJsonMessenger(IPEndPoint endpoint)
        {
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

        #region Implementation of IDisposable

        public void Dispose()
        {
            reader?.Dispose();
            reader = null;

            netStream?.Dispose();
            netStream = null;

            client?.Dispose();
            client = null;
        }

        #endregion

        #region INetMessenger

        public Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SendAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private void SendMessage(JsonValue message)
        {
            message.SerializeTo(netStream);
            netStream.WriteByte(0x0d);
            netStream.WriteByte(0x0a);
        }

        #endregion
    }
}