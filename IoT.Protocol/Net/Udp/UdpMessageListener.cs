using System.Net;

namespace IoT.Protocol.Net.Udp
{
    public abstract class UdpMessageListener : MessageListener<byte[]>
    {
        protected readonly IPEndPoint GroupEndpoint;
        private INetMessageReceiver<byte[]> receiver;

        protected UdpMessageListener(IPEndPoint groupEndpoint)
        {
            GroupEndpoint = groupEndpoint;
        }

        #region Overrides of MessageReceiver<byte[]>

        protected override INetMessageReceiver<byte[]> Receiver
        {
            get { return receiver; }
        }

        #endregion

        protected override void OnConnect()
        {
            receiver = new UdpBroadcastMessageReceiver(GroupEndpoint);

            base.OnConnect();
        }
    }
}