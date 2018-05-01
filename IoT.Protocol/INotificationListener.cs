namespace IoT.Protocol
{
    public interface INotificationListener<TNotification>
    {
         void Notify(TNotification notification);
    }
}