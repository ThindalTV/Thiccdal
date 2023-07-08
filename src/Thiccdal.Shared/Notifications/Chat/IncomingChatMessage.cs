namespace Thiccdal.Shared.Notifications.Chat;
public class IncomingChatMessage : ChatMessage
{
    public IncomingChatMessage(string source, string channel, string sender, DateTime timestamp, string message) : base(source, channel, sender, timestamp, message)
    {
        
    }
}
