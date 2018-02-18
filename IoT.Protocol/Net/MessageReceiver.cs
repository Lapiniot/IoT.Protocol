using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    /// <summary>
    /// Abstract base class for
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageReceiver<TMessage>
    {
        private readonly object syncRoot = new object();
        private bool connected;
        private bool disposed;
        protected abstract INetMessageReceiver<TMessage> Receiver { get; }

        /// <summary>
        /// Disposes object instance
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Establishes logical connection with IoT device: sets up udp client and starts listening for incoming datagrams.
        /// Must be called before control message sending.
        /// </summary>
        public void Connect()
        {
            CheckDisposed();

            if(!connected)
            {
                lock(syncRoot)
                {
                    if(!connected)
                    {
                        OnConnect();

                        connected = true;
                    }
                }
            }
        }

        /// <summary>
        /// Closes logical connection: stops listening for incoming datagrams and frees up udp client allocated resources.
        /// </summary>
        public void Close()
        {
            if(connected)
            {
                lock(syncRoot)
                {
                    if(connected)
                    {
                        OnClose();

                        Receiver?.Dispose();

                        connected = false;
                    }
                }
            }
        }

        protected abstract void OnConnect();

        protected abstract void OnClose();

        protected void CheckConnected()
        {
            if(!connected) throw new InvalidOperationException("Not connected");
        }

        protected void CheckDisposed()
        {
            if(disposed) throw new ObjectDisposedException("this");
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(!disposed)
                {
                    Close();

                    disposed = true;
                }
            }
        }

        protected Task<(IPEndPoint RemoteEP, TMessage Message)> ReceiveAsync(CancellationToken cancellationToken)
        {
            CheckConnected();
            CheckDisposed();

            return Receiver.ReceiveAsync(cancellationToken);
        }
    }
}