namespace daytoday.API.Core
{
    public interface INotification { }

    public interface INotificationHandler<TNotification>
        where TNotification : INotification
    {
        Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
    }
}
