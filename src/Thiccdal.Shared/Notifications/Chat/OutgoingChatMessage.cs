using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications.Chat;
public class OutgoingChatMessage : ChatMessage, INotification
{
    public OutgoingChatMessage(Source source, string channel, string sender, DateTime timestamp, string message) : base(source, channel, sender, timestamp, message)
    {
    }
}
