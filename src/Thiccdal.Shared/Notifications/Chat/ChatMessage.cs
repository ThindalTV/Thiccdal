using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications.Chat;
public class ChatMessage : INotification
{
    public Source Source { get; }
    public string Channel { get; }
    public string Sender { get; }
    public DateTime Timestamp { get; }

    public string Message { get; }

    public ChatMessage(Source source, string channel, string sender, DateTime timestamp, string message)
    {
        Source = source;
        Channel = channel ?? throw new ArgumentNullException(nameof(channel), "Can't send a message to an unknown channel");
        Sender = sender ?? throw new ArgumentNullException(nameof(sender), "Can't send a message without a sender");
        Timestamp = timestamp;
        Message = message ?? throw new ArgumentNullException(nameof(message), "Can't send a message without any content");
    }
}
