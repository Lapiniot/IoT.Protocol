using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Net
{
    public abstract class DispatchingMessenger<TRequestMessage, TResponseMessage> : MessageListener<TResponseMessage>
    {
        private INetMessenger<TRequestMessage, TResponseMessage> messenger;

        #region Overrides of MessageReceiver<TResponseMessage>

        protected override INetMessageReceiver<TResponseMessage> Receiver
        {
            get { return messenger; }
        }

        #endregion

        public Task SendAsync(TRequestMessage message, CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckConnected();

            return messenger.SendAsync(message, cancellationToken);
        }

        #region Overrides of MessageListener<TResponseMessage>

        protected override void OnConnect()
        {
            messenger = CreateNetMessenger();

            base.OnConnect();
        }

        #endregion

        protected abstract INetMessenger<TRequestMessage, TResponseMessage> CreateNetMessenger();
    }
}