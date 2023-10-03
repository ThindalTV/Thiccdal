using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications;

[Flags]
public enum Source
{
    All = Twitch | Discord | Twitter,
    Twitch = 1,
    Discord = 2, // WIP
    Twitter = 4 // WIP
}

[Flags]
public enum UserFlags
{
    None = 0,
    Vip = 1,
    Subscriber = 2,
    Moderator = 4,
    TwitchStaff = 8,
    TwitchPartner = 16,
    TwitchBroadcaster = 32
}

[Flags]
public enum MessageFlags
{
    None = 0,
    UsersFirstMessage = 1,
    Highlighted = 2,
    Donation = 4
};

public class UserInfo
{
    public Source UserSource { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public UserFlags Flags { get; set; }
    public int SubscribedForMonths { get; set; }
}

public class TwitchChatNotification : INotification
{
    public Source ChatSource { get; set; }
    public string MessageId { get; set; }
    public DateTime Datetime { get; set; }
    public string Channel { get; set; }
    public UserInfo User { get; set; }
    public MessageFlags Flags { get; set; }
    public string Message { get; set; }
    public int Bits { get; set; }
    public decimal BitsInDollars { get; set; }

}
