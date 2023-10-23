using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Chat;

namespace Thiccdal.Todo;

internal record Subscriber(string UserName, string Channel);

internal class TodoItem
{

    public TodoItem(string todoText)
    {
        TodoText = todoText;
        Created = DateTime.Now;
    }

    public override string ToString()
    {
        return $"{(Completed.HasValue ? "[X]" : "[ ]")} ({Id}): {TodoText}";
    }

    public DateTime? Completed { get; set; }
    public DateTime Created { get; set; }
    public int Id { get; set; }
    public bool IsCompleted => Completed.HasValue;
    public string TodoText { get; set; }
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

    private async Task TodoMessageHandler(TwitchChatNotification notification, CancellationToken token)
    {
        var parts = notification.Message.Split(' ');

        string command = String.Empty;
        if (parts.Length > 1)
        {
            command = parts[1];
        }

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
            case "":
            case "help":
            case "commands":
            case "-help":
            case "-commands":
            case "-h":
            case "-c":
                var message = new List<string>() { "Available commands: add, list, complete, remove, clear",
                "!todo add <text> - Adds a todo item to your todo list.",
                "!todo list - Lists your uncompleted todo items together with the id number. Can also be followed by all to display all todo items, including completed ones",
                "!todo complete <id> - Marks a todo item as completed.",
                "!todo remove <id> - Removes a todo item from your todo list as if it never existed.",
                "!todo clear - Removes ALL todo items in your todo list."};
                foreach (var m in message)
                {
                    await _eventAggregator.Publish(
                        new OutgoingChatMessage(
                        notification.ChatSource,
                        notification.Channel,
                        "Thiccdal",
                        DateTime.Now,
                        m));
                }
                break;
            case "add":
                int maxId;
                if (items.Any())
                {
                    maxId = items.Max(x => x.Id);
                }
                else
                {
                    maxId = 0;
                }

                var todoText = string.Join(' ', parts.Skip(2));
                var todoItem = new TodoItem(todoText) { Id = maxId + 1 };
                items.Add(todoItem);
                await _eventAggregator.Publish(
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
                    await _eventAggregator.Publish(
                        new OutgoingChatMessage(
                        notification.ChatSource,
                        notification.Channel,
                        "Thiccdal",
                        DateTime.Now,
                        $"Completed todo item: {item.TodoText}"));
                }
                else
                {
                    await _eventAggregator.Publish(
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
                    await _eventAggregator.Publish(
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
                    await _eventAggregator.Publish(
                        new OutgoingChatMessage(
                            notification.ChatSource,
                            notification.Channel,
                            "Thiccdal",
                            DateTime.Now,
                            $"Todo items for {notification.User.Name}:"));
                    foreach (var todo in list)
                    {
                        await _eventAggregator.Publish(
                            new OutgoingChatMessage(
                                notification.ChatSource,
                                notification.Channel,
                                "Thiccdal",
                                DateTime.Now,
                                todo.ToString()));
                    }
                }
                else
                {
                    await _eventAggregator.Publish(
                    new OutgoingChatMessage(notification.ChatSource,
                        notification.Channel,
                        "Thiccdal",
                        DateTime.Now,
                        $"No todo items for {notification.User.Name}"));
                }
                break;
            case "clear":
                items.Clear();
                await _eventAggregator.Publish(
                    new OutgoingChatMessage(notification.ChatSource,
                        notification.Channel,
                        "Thiccdal",
                        DateTime.Now,
                        $"Cleared todo items for {notification.User.Name}"));
                break;
        }
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

    public Task Stop()
    {
        _eventAggregator.Unsubscribe<TwitchChatNotification>(this);
        return Task.CompletedTask;
    }
}
