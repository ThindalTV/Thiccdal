using System.Linq;
using System.Threading;
using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Chat;
using Thiccdal.Shared.Repositories;

namespace ResponderService;

public class Response
{
    public string Command { get; set; }
    public string ResponseText { get; set; }
}

public class ChatResponderService : IService, IEventSubscriber
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IRepository _repository;

    public ChatResponderService(IEventAggregator eventAggregator, IRepository repository)
    {
        _eventAggregator = eventAggregator;
        _repository = repository;
    }



    public Task Start(CancellationToken cancellationToken)
    {
        _eventAggregator.Subscribe<TwitchChatNotification>(this,
            (msg) => msg.Message.StartsWith("!"),
            MessageHandler);

        _eventAggregator.Subscribe<TwitchChatNotification>(this,
            (msg) => msg.Message.StartsWith("!resp"),
            CommandHandler);

        return Task.CompletedTask;
    }

    private async Task MessageHandler(TwitchChatNotification msg, CancellationToken cancellationToken)
    {
        var responses = await _repository.Get<List<Response>>("ChatResponses", msg.Channel, cancellationToken)
            ?? new List<Response>();

        var responseText = responses.FirstOrDefault(r => r.Command == msg.Message.Substring(1))?.ResponseText;
        if (responseText != null)
        {
            await _eventAggregator.Publish(new OutgoingChatMessage(msg.ChatSource, msg.Channel, "Thiccdal", DateTime.Now, responseText), this, cancellationToken);
            return;
        }
    }

    private async Task CommandHandler(TwitchChatNotification msg, CancellationToken cancellationToken)
    {
        var responses = await _repository.Get<List<Response>>("ChatResponses", msg.Channel, cancellationToken)
            ?? new List<Response>();

        var dukas = new[] { "dukasoft", "thindal" };
        var dukaName = msg.User.Name.ToLower();
        if (!dukas.Contains(dukaName))
            return;

        var parts = msg.Message.Split(' ');
        if (parts.Length < 3)
            return;
        var action = parts[1].ToLower();

        var trigger = parts[2].ToLower();
        var response = string.Join(' ', parts.Skip(3));

        switch (action)
        {
            case "add":
                responses.Add(new Response() { Command = trigger, ResponseText = response });
                await _repository.Set("ChatResponses", msg.Channel, responses, cancellationToken);
                await _eventAggregator.Publish(new OutgoingChatMessage(msg.ChatSource, msg.Channel, "Thiccdal", DateTime.Now, "Added response"), this, cancellationToken);
                break;
            case "remove":
                var toRemove = responses.FirstOrDefault(r => r.Command == trigger);
                if (toRemove != null)
                {
                    responses.Remove(toRemove);
                    await _repository.Set("ChatResponses", msg.Channel, responses, cancellationToken);
                    await _eventAggregator.Publish(new OutgoingChatMessage(msg.ChatSource, msg.Channel, "Thiccdal", DateTime.Now, "Removed response"), this, cancellationToken);
                    break;
                }
                await _eventAggregator.Publish(new OutgoingChatMessage(msg.ChatSource, msg.Channel, "Thiccdal", DateTime.Now, "Could not find response"), this, cancellationToken);
                break;
            default:
                await _eventAggregator.Publish(new OutgoingChatMessage(msg.ChatSource, msg.Channel, "Thiccdal", DateTime.Now, "Unknown action"), this, cancellationToken);
                break;
        }
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}
