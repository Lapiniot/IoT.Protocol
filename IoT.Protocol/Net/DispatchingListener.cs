using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
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

        protected override Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
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