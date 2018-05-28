using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;

namespace IoT.Protocol
{
    public abstract class DispatchingListener : DataListener
    {
        private INetMessenger messenger;

        public Task SendAsync(byte[] message, int offset, int size, CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckConnected();

            return messenger.SendAsync(message, offset, size, cancellationToken);
        }

        protected abstract INetMessenger CreateNetMessenger();

        #region Overrides of DataListener

        public override async Task<(int Size, IPEndPoint RemoteEP)> ReceiveAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            CheckDisposed();
            CheckConnected();

            var valueTask = messenger.ReceiveAsync(buffer, cancellationToken);

            return valueTask.IsCompleted ? valueTask.Result : await valueTask.ConfigureAwait(false);
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