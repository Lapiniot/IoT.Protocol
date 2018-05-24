namespace IoT.Protocol.Interfaces
{
    /// <summary>
    /// Provides a possibility to exchange binary messages between client and remote IP endpoint
    /// </summary>
    public interface INetMessenger : IMessageReceiver, IMessageSender
    {
    }
}