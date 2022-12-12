namespace Thiccdal.Shared.EventAggregator;

public interface IEventAggregator
{
    Task Publish<TPublishNotificationType>(TPublishNotificationType notification, CancellationToken cancellationToken = default) where TPublishNotificationType : Notification;
    void Subscribe<TSubscribeNotificationType>(IEventSubscriber subscriber, Func<TSubscribeNotificationType, CancellationToken, Task> handler) where TSubscribeNotificationType : Notification;

}
