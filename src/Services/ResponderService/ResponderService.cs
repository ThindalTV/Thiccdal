using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Chat;

namespace ResponderService;

public class ChatResponderService : IService, IEventSubscriber
{
    readonly IEventAggregator _eventAggregator;
    public ChatResponderService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }



    public Task Start(CancellationToken cancellationToken)
    {
        _eventAggregator.Subscribe<TwitchChatNotification>(this,
            (msg) => msg.Message.StartsWith("!"),
            MessageRespondeHandler);
        return Task.CompletedTask;
    }

    private Task MessageRespondeHandler(TwitchChatNotification msg, CancellationToken cancellationToken)
    {
        if (string.Equals(msg.Message, "!hi", StringComparison.CurrentCultureIgnoreCase))
        {
            if (string.Equals(msg.User.Name, "dukasoft", StringComparison.CurrentCultureIgnoreCase))
            {                _eventAggregator.Publish(
                               new OutgoingChatMessage(msg.ChatSource, "thindal", "Thiccdal", DateTime.Now, "Hi there Duka you lovely hooman!"));

            }
            else
            {
                _eventAggregator.Publish(
                        new OutgoingChatMessage(msg.ChatSource, "thindal", "Thiccdal", DateTime.Now, "Hi there!"));
            }
        }
        else if (
            (string.Equals(msg.User.Name, "thindal", StringComparison.CurrentCultureIgnoreCase)
                || string.Equals(msg.User.Name, "dukasoft", StringComparison.CurrentCultureIgnoreCase))
            && msg.Message.StartsWith("!so ", StringComparison.CurrentCultureIgnoreCase))
        {
            var channel = msg.Message.Split(' ')[1];
            string outMessage = $"Did you know that {channel} streams and are awesome? Because they are!";
            _eventAggregator.Publish(new OutgoingChatMessage(msg.ChatSource, msg.Channel, "Thindal", DateTime.Now, outMessage));

        }
            return Task.CompletedTask;
    }

    public Task Stop()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
