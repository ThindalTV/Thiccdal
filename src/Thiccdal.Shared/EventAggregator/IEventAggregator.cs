namespace Thiccdal.Shared.EventAggregator;

public interface IEventAggregator
{
    Task Publish<TPublishNotificationType>(TPublishNotificationType notification, IEventSubscriber? @this = null, CancellationToken cancellationToken = default) 
        where TPublishNotificationType : INotification;
    public void Subscribe<TSubscribeNotification>(IEventSubscriber subscriber, Func<TSubscribeNotification, CancellationToken, Task> handler)
        where TSubscribeNotification : notnull, INotification;
    void Subscribe<TSubscribeNotificationType>(IEventSubscriber subscriber, Func<TSubscribeNotificationType, bool>? predicate, Func<TSubscribeNotificationType, CancellationToken, Task> handler)
        where TSubscribeNotificationType : notnull, INotification;
    void Unsubscribe<TSubscribeNotification>(IEventSubscriber subscriber) 
        where TSubscribeNotification : INotification;
}