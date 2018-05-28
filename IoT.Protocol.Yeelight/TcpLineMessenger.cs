using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol.Yeelight
{
    public class TcpLineMessenger : INetMessenger
    {
        private const byte CR = 0x0d;
        private const byte LF = 0x0a;

        private byte[] eol = { CR, LF };
        private Socket socket;
        private readonly IPEndPoint endpoint;

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
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            socket?.Dispose();
        }

        #endregion

        #region INetMessenger

        public async ValueTask<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            var window = buffer.AsMemory();
            var total = 0;

            if(reminder.Length > 0)
            {
                if(ContainsEOL(reminder, out var i))
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
                var size = await socket.ReceiveAsync(window, SocketFlags.None, cancellationToken);
                var received = window.Slice(0, size);

                if(ContainsEOL(received, out var i))
                {
                    total += i;
                    reminder = received.Slice(i + 2).ToArray();
                    return (total, endpoint);
                }
                else
                {
                    total += size;
                    window = window.Slice(size);
                }

            } while(true);
        }

        bool ContainsEOL(Memory<byte> memory, out int index)
        {
            return (index = memory.Span.IndexOf(CR)) > 0 && index < memory.Length - 1 && memory.Span[index + 1] == LF;
        }

        public async Task SendAsync(byte[] buffer, int offset, int size, CancellationToken cancellationToken)
        {
            await socket.SendAsync(buffer, offset, size, cancellationToken).ConfigureAwait(false);
            if(!(buffer[offset + size - 2] == CR && buffer[offset + size - 1] == LF))
            {
                await socket.SendAsync(eol, 0, 2, cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion
    }
}