using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Chat;

namespace Thiccdal.Todo;

internal record Subscriber(string UserName, string Channel);

internal class TodoItem
{
    public int Id { get; set; }
    public string TodoText { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Completed { get; set; }
    public bool IsCompleted => Completed.HasValue;

    public TodoItem(string todoText)
    {
        TodoText = todoText;
        Created = DateTime.Now;
    }

    public override string ToString()
    {
        return $"{(Completed.HasValue ? "[X]" : "[ ]")} ({Id}): {TodoText}";
    }
}

public class TodoService : IService, IEventSubscriber
{
    private readonly IEventAggregator _eventAggregator;
    private readonly Dictionary<Subscriber, List<TodoItem>> _todoItems;

    public TodoService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;

        _todoItems = new Dictionary<Subscriber, List<TodoItem>>();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task Start(CancellationToken cancellationToken)
    {
        _eventAggregator.Subscribe<TwitchChatNotification>(
            this, (notification) =>
            notification.ChatSource.HasFlag(Source.Twitch)
                && notification.Message.StartsWith("!todo", StringComparison.CurrentCultureIgnoreCase),
            TodoMessageHandler);

        return Task.CompletedTask;
    }

    private Task TodoMessageHandler(TwitchChatNotification notification, CancellationToken token)
    {
        var parts = notification.Message.Split(' ');

        var command = parts[1];

        List<TodoItem> items;
        if (_todoItems.Keys.Contains(new Subscriber(notification.User.Name, notification.Channel)))
        {
            items = _todoItems[new Subscriber(notification.User.Name, notification.Channel)];
        }
        else
        {
            items = new List<TodoItem>();
            _todoItems.Add(new Subscriber(notification.User.Name, notification.Channel), items);
        }

        var todoItemsToOutput = new List<TodoItem>();
        switch (command.ToLower())
        {
            case "add":
                int maxId;
                if( items.Any())
                {
                    maxId = items.Max(x => x.Id);
                } else
                {
                    maxId = 0;
                }

                var todoText = string.Join(' ', parts.Skip(2));
                var todoItem = new TodoItem(todoText) { Id = maxId + 1 };
                items.Add(todoItem);
                _eventAggregator.Publish(
                    new OutgoingChatMessage(
                        notification.ChatSource,
                        notification.Channel,
                        "Thiccdal",
                        DateTime.Now,
                        $"Added todo item: ({todoItem.Id}): {todoText}"));
                break;
            case "complete":
                var id = int.Parse(parts[2]);
                var item = items.FirstOrDefault(x => x.Id == id);
                if (item != null)
                {
                    item.Completed = DateTime.Now;
                    _eventAggregator.Publish(
                        new OutgoingChatMessage(
                        notification.ChatSource,
                        notification.Channel,
                        "Thiccdal",
                        DateTime.Now,
                        $"Completed todo item: {item.TodoText}"));
                } else
                {
                    _eventAggregator.Publish(
                        new OutgoingChatMessage(notification.ChatSource, 
                            notification.Channel, 
                            "Thiccdal", 
                            DateTime.Now, 
                            $"Could not find todo item with id: {id}"));
                }
                break;
            case "remove":
                var idToRemove = int.Parse(parts[2]);
                var itemToRemove = items.FirstOrDefault(x => x.Id == idToRemove);
                if (itemToRemove != null)
                {
                    items.Remove(itemToRemove);
                    _eventAggregator.Publish(
                        new OutgoingChatMessage(notification.ChatSource,
                            notification.Channel,
                            "Thiccdal",
                            DateTime.Now,
                            $"Removed todo item: {itemToRemove.TodoText}"));
                }
                break;
            case "list":
                var displayAll = parts.Length > 2 && string.Equals(parts[2], "all", StringComparison.OrdinalIgnoreCase);
                var list = items.Where(x => displayAll || !x.IsCompleted);

                if (list.Any())
                {
                    _eventAggregator.Publish(
                        new OutgoingChatMessage(
                            notification.ChatSource,
                            notification.Channel,
                            "Thiccdal",
                            DateTime.Now,
                            $"Todo items for {notification.User.Name}:"));
                    foreach (var todo in list)
                    {
                        _eventAggregator.Publish(
                            new OutgoingChatMessage(
                                notification.ChatSource,
                                notification.Channel,
                                "Thiccdal",
                                DateTime.Now,
                                todo.ToString()));
                    }
                } else
                {
                    _eventAggregator.Publish(
                    new OutgoingChatMessage(notification.ChatSource,
                        notification.Channel,
                        "Thiccdal",
                        DateTime.Now,
                        $"No todo items for {notification.User.Name}"));
                }
                break;
            case "clear":
                items.Clear();
                _eventAggregator.Publish(
                    new OutgoingChatMessage(notification.ChatSource,
                        notification.Channel,
                        "Thiccdal",
                        DateTime.Now,
                        $"Cleared todo items for {notification.User.Name}"));
                break;
        }
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }
}
