using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.EventAggregator;

public class EventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, List<Event>> _handlers;

    public EventAggregator()
    {
        _handlers = new Dictionary<Type, List<Event>>();
    }

    public Task Publish<TPublishNotificationType>(TPublishNotificationType notification, IEventSubscriber? @this = null, CancellationToken cancellationToken = default) where TPublishNotificationType : Notification
    {
        // Locate dictionary entry for type T and call all handlers
        if (_handlers.TryGetValue(typeof(TPublishNotificationType), out var events))
        {
            List<Task> tasks = new();
            var eventsToRemove = new List<Event>();
            // Execute each handler
            foreach (Event @event in events)
            {
                try
                {
                    if (@this == null || @event.Subscriber != @this)
                    {
                        // Execute the handler
                        tasks.Add(@event.Handler(notification, cancellationToken));
                    }
                }
                catch (ObjectDisposedException)
                {
                    eventsToRemove.Add(@event);
                }
            }
            if (eventsToRemove.Any())
            {
                events.RemoveAll(e => eventsToRemove.Contains(e));
            }

            return Task.WhenAll(tasks);
        }
        return Task.CompletedTask;
    }

    public void Subscribe<TSubscribeNotification>(IEventSubscriber subscriber, Func<TSubscribeNotification, CancellationToken, Task> handler)
        where TSubscribeNotification : Notification
    {
        var @event = new Event()
        {
            Subscriber = subscriber,
            Handler = (Notification arg1, CancellationToken arg2) => { return handler((TSubscribeNotification)arg1, arg2); }
        };
        // Add handler to list for TSubscribeNotification
        if (_handlers.TryGetValue(typeof(TSubscribeNotification), out List<Event> events))
        {
            events.Add(@event);
        }
        else
        {
            _handlers.Add(typeof(TSubscribeNotification),
                new List<Event>() { @event });
        }
    }

    public void Unsubscribe<TSubscribeNotification>(IEventSubscriber subscriber)
        where TSubscribeNotification : Notification
    {
        // Locate the event list for this notification type
        if (_handlers.TryGetValue(typeof(TSubscribeNotification), out List<Event> events))
        {
            var eventsToRemove = new List<Event>();
            // Look for the event with the subscriber and remove it
            foreach (Event @event in events)
            {
                if (@event.Subscriber == subscriber)
                {
                    eventsToRemove.Add(@event);
                }
            }
            // Remove events from the list
            if (eventsToRemove.Any())
            {
                events.RemoveAll(e => eventsToRemove.Contains(e));
            }
        }
    }
}

internal struct Event
{
    public required IDisposable Subscriber { get; init; }
    public required Func<Notification, CancellationToken, Task> Handler { get; init; }
}
