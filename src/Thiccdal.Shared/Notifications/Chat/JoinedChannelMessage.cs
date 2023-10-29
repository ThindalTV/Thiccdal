using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications.Chat;

public class JoinedChannelMessage : INotification
{
    public Source Source { get; set; }
    public string ChannelName { get; set; }

    public JoinedChannelMessage(Source source, string channelName)
    {
        Source = source;
        ChannelName = channelName;
    }
}
