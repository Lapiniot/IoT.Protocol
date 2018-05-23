namespace IoT.Protocol.Net
{
    /// <summary>
    /// Provides a possibility to exchange messages between client and remote IP endpoint
    /// </summary>
    public interface INetMessenger : IMessageReceiver, IMessageSender
    {
    }
}