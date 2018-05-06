namespace IoT.Protocol
{
    public interface INotificationListener<in TNotification>
    {
        void Notify(TNotification notification);
    }
}