using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol
{
    public abstract class DispatchingListener : DataListener
    {
        private INetMessenger messenger;

        public Task SendAsync(byte[] message, CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckConnected();

            return messenger.SendAsync(message, cancellationToken);
        }

        protected abstract INetMessenger CreateNetMessenger();

        #region Overrides of DataListener

        public override Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckConnected();

            return messenger.ReceiveAsync(buffer, cancellationToken);
        }

        protected override void OnConnect()
        {
            messenger = CreateNetMessenger();

            base.OnConnect();
        }

        protected override void OnClose()
        {
            base.OnClose();

            messenger.Dispose();

            messenger = null;
        }

        #endregion
    }
}