using System;
using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications;
public class RawData : Notification
{
    public string Context { get; }
    public DateTime DateTime { get; }
    public string Data { get; }

    public RawData(string context, DateTime dateTime, string data)
    {
        Context = context;
        DateTime = dateTime;
        Data = data;
    }

}
