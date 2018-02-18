namespace IoT.Protocol.Net
{
    /// <summary>
    /// Provides a possibility to exchange messages between client and remote IP endpoint
    /// </summary>
    /// <typeparam name="TRequestMessage">Type of request messages</typeparam>
    /// <typeparam name="TResponseMessage">Type of response messages</typeparam>
    public interface INetMessenger<in TRequestMessage, TResponseMessage> :
        INetMessageReceiver<TResponseMessage>,
        INetMessageSender<TRequestMessage>
    {
    }
}