using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.EventAggregator;

public class EventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, Events> _handlers;

    public EventAggregator()
    {
        _handlers = new Dictionary<Type, Events>();
    }

    public Task Publish<TPublishNotificationType>(TPublishNotificationType notification, IEventSubscriber? @this = null, CancellationToken cancellationToken = default)
        where TPublishNotificationType : INotification
    {
        // Locate dictionary entry for type T and call all handlers
        if (_handlers.TryGetValue(typeof(TPublishNotificationType), out var events))
        {
            List<Task> tasks = new();
            var eventsToRemove = new List<Event>();
            // Execute each handler
            foreach (var @event in events.EventsList)
            {
                try
                {
                    if (@this == null || @event.Subscriber != @this)
                    {
                        // Check if the notification matches the predicate
                        if (@event.Predicate(notification))
                        {
                            // Execute the handler
                            tasks.Add(@event.Handler(notification, cancellationToken));
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    eventsToRemove.Add(@event);
                }
            }
            if (eventsToRemove.Any())
            {
                events.EventsList.RemoveAll(eventsToRemove.Contains);
            }

            return Task.WhenAll(tasks);
        }
        return Task.CompletedTask;
    }

    public void Subscribe<TSubscribeNotification>(IEventSubscriber subscriber, Func<TSubscribeNotification, CancellationToken, Task> handler)
    where TSubscribeNotification : notnull, INotification
    {
        Subscribe<TSubscribeNotification>(subscriber, null, (TSubscribeNotification arg1, CancellationToken arg2) => handler((TSubscribeNotification)arg1, arg2));
    }

    public void Subscribe<TSubscribeNotification>(IEventSubscriber subscriber, Func<TSubscribeNotification, bool>? predicate, Func<TSubscribeNotification, CancellationToken, Task> handler)
        where TSubscribeNotification : notnull, INotification
    {
        var @event = new Event()
        {
            Subscriber = subscriber,
            Predicate = (INotification notification) => predicate != null ? predicate((TSubscribeNotification)notification) : true,
            Handler = (INotification arg1, CancellationToken arg2) => handler((TSubscribeNotification)arg1, arg2)
        };
        // Add handler to list for TSubscribeNotification
            if (_handlers.TryGetValue(typeof(TSubscribeNotification), out var events))
        {
            events.EventsList.Add(@event);
        }
        else
        {
            _handlers.Add(typeof(TSubscribeNotification),
                new Events() { EventsList = new List<Event>() { @event } });
        }
    }

    public void Unsubscribe<TSubscribeNotification>(IEventSubscriber subscriber)
        where TSubscribeNotification : notnull, INotification
    {
        // Locate the event list for this notification type
        if (_handlers.TryGetValue(typeof(TSubscribeNotification), out var events))
        {
            var eventsToRemove = new List<Event>();
            // Look for the event with the subscriber and remove it
            foreach (Event @event in events.EventsList)
            {
                if (@event.Subscriber == subscriber)
                {
                    eventsToRemove.Add(@event);
                }
            }
            // Remove events from the list
            if (eventsToRemove.Any())
            {
                events.EventsList.RemoveAll(eventsToRemove.Contains);
            }
        }
    }
}

internal class Events
{
    public List<Event> EventsList { get; init; }

    public Events()
    {
        EventsList = new List<Event>();
    }
}

internal class Event
{
    public required IDisposable Subscriber { get; init; }
    public required Func<INotification, bool> Predicate { get; init; }
    public required Func<INotification, CancellationToken, Task> Handler { get; init; }
}
