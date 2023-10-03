using Thiccdal.Shared;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications.Chat;

namespace TwitchShoutoutService;

public class ShoutoutService : IService, IEventSubscriber
{
    private readonly IEventAggregator _eventAggregator;

    public ShoutoutService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }

    public Task Start(CancellationToken cancellationToken)
    {
        _eventAggregator.Subscribe<IncomingChatMessage>(this,
            msg =>
            msg.Source.HasFlag(Source.Twitch)
            && string.Equals(msg.Sender, "thindal", StringComparison.CurrentCultureIgnoreCase)
            && msg.Message.StartsWith("!so ", StringComparison.CurrentCultureIgnoreCase), HandleShoutout);
        return Task.CompletedTask;
    }

    private Task HandleShoutout(IncomingChatMessage message, CancellationToken token)
    {
        var channel = message.Message.Split(' ')[1];
        _eventAggregator.Publish(new ShoutoutCommand(Source.Twitch, message.Channel, "Thindal", DateTime.Now, channel));
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}
