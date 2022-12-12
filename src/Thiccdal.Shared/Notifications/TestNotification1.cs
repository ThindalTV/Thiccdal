using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications;
public class TestNotification1 : Notification
{
    public readonly string Message;

    public TestNotification1(string message)
    {
        Message = $"Message type 1: {message}";
    }
}
