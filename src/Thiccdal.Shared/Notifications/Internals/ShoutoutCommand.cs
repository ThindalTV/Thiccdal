using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications;
public class ShoutoutCommand : INotification
{
    public ShoutoutCommand(Source source, string channel, string sender, DateTime now, string reciever)
    {
        Source = source;
        Channel = channel;
        Sender = sender;
        Now = now;
        Reciever = reciever;
    }

    public Source Source { get; }
    public string Channel { get; }
    public string Sender { get; }
    public DateTime Now { get; }
    public string Reciever { get; }
}
