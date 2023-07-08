namespace Thiccdal.Shared.Notifications.Chat;
public class OutgoingChatMessage : ChatMessage
{
    public OutgoingChatMessage(string source, string channel, string sender, DateTime timestamp, string message) : base(source, channel, sender, timestamp, message)
    {
    }
}
