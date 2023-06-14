using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications.Internals;
public class LogMessageNotification : Notification
{
    public string Sender { get; }
    public string Message { get; }
    public DateTime Timestamp { get; set; }

    public LogMessageNotification(string sender, string message)
    {
        Sender = sender;
        Message = message;
        Timestamp = DateTime.Now;
    }
}
