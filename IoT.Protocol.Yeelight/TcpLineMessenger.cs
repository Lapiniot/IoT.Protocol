using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol.Yeelight
{
    public class TcpLineMessenger : INetMessenger
    {
        private readonly byte[] endOfLineMarker = {0x0d, 0x0a};

        private readonly IPEndPoint endpoint;
        private readonly Socket socket;

        private Memory<byte> reminder;

        public TcpLineMessenger(IPEndPoint endpoint)
        {
            this.endpoint = endpoint;
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(endpoint);
            }
            catch
            {
                socket?.Dispose();
                throw;
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            socket.Dispose();
        }

        #endregion

        #region INetMessenger

        public async ValueTask<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            var window = buffer.AsMemory();
            var total = 0;

            if(reminder.Length > 0)
            {
                if(reminder.TryFindEolMarker(out var i))
                {
                    reminder.Slice(0, i).CopyTo(window);
                    reminder = reminder.Slice(i + 2);
                    return (i, endpoint);
                }

                reminder.CopyTo(window);
                total = reminder.Length;
                window = window.Slice(total);
                reminder = Memory<byte>.Empty;
            }

            do
            {
                var size = await socket.ReceiveAsync(window, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                var received = window.Slice(0, size);

                if(received.TryFindEolMarker(out var i))
                {
                    total += i;
                    reminder = received.Slice(i + 2).ToArray();
                    return (total, endpoint);
                }

                total += size;
                window = window.Slice(size);
            } while(true);
        }

        public async Task SendAsync(byte[] buffer, int offset, int size, CancellationToken cancellationToken)
        {
            await socket.SendAsync(buffer, offset, size, cancellationToken).ConfigureAwait(false);
            if(!(buffer[offset + size - 2] == 0x0d && buffer[offset + size - 1] == 0x0a))
            {
                await socket.SendAsync(endOfLineMarker, 0, 2, cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion
    }
}