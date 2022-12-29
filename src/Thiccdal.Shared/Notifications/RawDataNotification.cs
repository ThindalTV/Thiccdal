using System;
using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications;
public class RawDataNotification : Notification
{
    public string Context { get; }
    public DateTime DateTime { get; }
    public string RawData { get; }

    public RawDataNotification(string context, DateTime dateTime, string rawData)
    {
        Context = context;
        DateTime = dateTime;
        RawData = rawData;
    }

}
