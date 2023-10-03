using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications.Chat;
public class OutgoingChatMessage : ChatMessage, INotification
{
    public OutgoingChatMessage(string source, string channel, string sender, DateTime timestamp, string message) : base(source, channel, sender, timestamp, message)
    {
    }
}
