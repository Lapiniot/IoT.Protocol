﻿using System;
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

        private readonly Socket socket;

        private Memory<byte> reminder;

        public TcpLineMessenger(IPEndPoint endpoint)
        {
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

        public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            var window = buffer;
            var total = 0;
            int index;
            if(!reminder.IsEmpty)
            {
                if((index = reminder.Span.IndexOfEOL()) >= 0)
                {
                    reminder.Slice(0, index).CopyTo(window);
                    reminder = reminder.Slice(index + 2);
                    return index;
                }

                reminder.CopyTo(window);
                total = reminder.Length;
                window = window.Slice(total);
                reminder = Memory<byte>.Empty;
            }

            while(!window.IsEmpty)
            {
                var valueTask = socket.ReceiveAsync(window, SocketFlags.None, cancellationToken);

                var size = valueTask.IsCompleted ? valueTask.Result : await valueTask.ConfigureAwait(false);

                var received = window.Slice(0, size);

                if((index = received.Span.IndexOfEOL()) >= 0)
                {
                    total += index;
                    reminder = received.Slice(index + 2).ToArray();
                    return total;
                }

                total += size;
                window = window.Slice(size);
            }

            throw new ArgumentException("Out of space in the " + nameof(buffer));
        }

        public async Task SendAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            await socket.SendAsync(buffer, cancellationToken).ConfigureAwait(false);
            var length = buffer.Length;
            if(!(buffer.Span[length - 2] == 0x0d && buffer.Span[length - 1] == 0x0a))
            {
                await socket.SendAsync(endOfLineMarker, 0, 2, cancellationToken).ConfigureAwait(false);
            }
        }

        #endregion
    }
}