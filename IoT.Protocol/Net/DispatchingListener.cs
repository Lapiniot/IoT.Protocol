using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    public abstract class DispatchingListener<TRequestMessage, TResponseMessage> : MessageListener<TResponseMessage>
    {
        private INetMessenger<TRequestMessage, TResponseMessage> messenger;

        public Task SendAsync(TRequestMessage message, CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckConnected();

            return messenger.SendAsync(message, cancellationToken);
        }

        protected abstract INetMessenger<TRequestMessage, TResponseMessage> CreateNetMessenger();

        #region Overrides of MessageListener<TResponseMessage>

        protected override Task<(IPEndPoint RemoteEP, TResponseMessage Message)> ReceiveAsync(CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckConnected();

            return messenger.ReceiveAsync(cancellationToken);
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