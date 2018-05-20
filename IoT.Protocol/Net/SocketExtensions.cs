using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Sockets.SocketFlags;

namespace IoT.Protocol.Net
{
    public static class SocketExtensions
    {
        public static async Task<int> SendAsync(this Socket socket, byte[] bytes, int offset, int size, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<int>();

            using(completionSource.Bind(cancellationToken))
            {
                try
                {
                    socket.BeginSend(bytes, offset, size, None, OnSendComplete, new AsyncState<int>(socket, completionSource));
                }
                catch(Exception exception)
                {
                    completionSource.TrySetException(exception);
                }

                return await completionSource.Task.ConfigureAwait(false);
            }
        }

        public static async Task<int> SendToAsync(this Socket socket, byte[] bytes, int offset, int size, EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<int>();

            using(completionSource.Bind(cancellationToken))
            {
                try
                {
                    socket.BeginSendTo(bytes, offset, size, None, remoteEndPoint, OnSendToComplete, new AsyncState<int>(socket, completionSource));
                }
                catch(Exception exception)
                {
                    completionSource.TrySetException(exception);
                }

                return await completionSource.Task.ConfigureAwait(false);
            }
        }

        public static Task<int> SendAsync(this Socket socket, byte[] bytes, CancellationToken cancellationToken)
        {
            return SendAsync(socket, bytes, 0, bytes.Length, cancellationToken);
        }

        public static Task<int> SendToAsync(this Socket socket, byte[] bytes, EndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            return SendToAsync(socket, bytes, 0, bytes.Length, remoteEndPoint, cancellationToken);
        }

        public static async Task<int> ReceiveAsync(this Socket socket, byte[] bytes, int offset, int size, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<int>();

            using(completionSource.Bind(cancellationToken))
            {
                try
                {
                    socket.BeginReceive(bytes, offset, size, None, OnReceiveComplete, new AsyncState<int>(socket, completionSource));
                }
                catch(Exception exception)
                {
                    completionSource.TrySetException(exception);
                }

                return await completionSource.Task.ConfigureAwait(false);
            }
        }

        public static Task<int> ReceiveAsync(this Socket socket, byte[] bytes, CancellationToken cancellationToken)
        {
            return ReceiveAsync(socket, bytes, 0, bytes.Length, cancellationToken);
        }

        public static async Task<(int Size, IPEndPoint RemoteEndPoint)> ReceiveFromAsync(this Socket socket, byte[] bytes, int offset, int size,
            IPEndPoint endPoint, CancellationToken cancellationToken)
        {
            var completionSource = new TaskCompletionSource<(int, IPEndPoint)>();

            using(completionSource.Bind(cancellationToken))
            {
                try
                {
                    EndPoint remoteEp = endPoint;
                    socket.BeginReceiveFrom(bytes, offset, size, None, ref remoteEp, OnReceiveFromComplete, new AsyncState<(int, IPEndPoint)>(socket, completionSource, endPoint));
                }
                catch(Exception exception)
                {
                    completionSource.TrySetException(exception);
                }

                return await completionSource.Task.ConfigureAwait(false);
            }
        }

        public static Task<(int Size, IPEndPoint RemoteEndPoint)> ReceiveFromAsync(this Socket socket, byte[] bytes, IPEndPoint endPoint, CancellationToken cancellationToken)
        {
            return ReceiveFromAsync(socket, bytes, 0, bytes.Length, endPoint, cancellationToken);
        }

        private static void OnSendComplete(IAsyncResult asyncResult)
        {
            var state = (AsyncState<int>)asyncResult.AsyncState;

            state.TryComplete(asyncResult, socket => socket.EndSend);
        }

        private static void OnSendToComplete(IAsyncResult asyncResult)
        {
            var state = (AsyncState<int>)asyncResult.AsyncState;

            state.TryComplete(asyncResult, socket => socket.EndSendTo);
        }

        private static void OnReceiveComplete(IAsyncResult asyncResult)
        {
            var state = (AsyncState<int>)asyncResult.AsyncState;

            state.TryComplete(asyncResult, socket => socket.EndReceive);
        }

        private static void OnReceiveFromComplete(IAsyncResult asyncResult)
        {
            var state = (AsyncState<(int Size, IPEndPoint RemoteEndPoint)>)asyncResult.AsyncState;

            state.TryComplete(asyncResult, OnComplete);

            (int Size, IPEndPoint RemoteEndPoint) OnComplete(Socket socket, IAsyncResult ar, IPEndPoint endPoint)
            {
                EndPoint ep = endPoint;
                return (socket.EndReceiveFrom(ar, ref ep), (IPEndPoint)ep);
            }
        }

        private class AsyncState<T>
        {
            public AsyncState(Socket socket, TaskCompletionSource<T> completionSource)
            {
                Socket = socket;
                CompletionSource = completionSource;
            }

            public AsyncState(Socket socket, TaskCompletionSource<T> completionSource, IPEndPoint endPoint) : this(socket, completionSource)
            {
                EndPoint = endPoint;
            }

            private IPEndPoint EndPoint { get; }
            private Socket Socket { get; }
            private TaskCompletionSource<T> CompletionSource { get; }

            public void TryComplete(IAsyncResult asyncResult, Func<Socket, Func<IAsyncResult, T>> selector)
            {
                try
                {
                    CompletionSource.TrySetResult(selector(Socket)(asyncResult));
                }
                catch(Exception exception)
                {
                    CompletionSource.TrySetException(exception);
                }
            }

            public void TryComplete(IAsyncResult asyncResult, Func<Socket, IAsyncResult, IPEndPoint, T> selector)
            {
                try
                {
                    CompletionSource.TrySetResult(selector(Socket, asyncResult, EndPoint));
                }
                catch(Exception exception)
                {
                    CompletionSource.TrySetException(exception);
                }
            }
        }
    }
}