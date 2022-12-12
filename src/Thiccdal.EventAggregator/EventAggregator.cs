using System;
using System.Collections.ObjectModel;
using System.Reflection.Metadata.Ecma335;
using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.EventAggregator;

public class EventAggregator : IEventAggregator
{
    private readonly Dictionary<Type, List<Event>> _handlers;

    public EventAggregator()
    {
        _handlers = new Dictionary<Type, List<Event>>();
    }

    public Task Publish<TPublishNotificationType>(TPublishNotificationType notification, CancellationToken cancellationToken = default) where TPublishNotificationType : Notification
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
                    // Execute the handler
                    tasks.Add(@event.Handler(notification, cancellationToken));
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
}

internal struct Event
{
    public required IDisposable Subscriber { get; init; }
    public required Func<Notification, CancellationToken, Task> Handler { get; init; }
}
