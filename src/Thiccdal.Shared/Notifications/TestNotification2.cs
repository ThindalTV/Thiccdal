﻿using Thiccdal.Shared.EventAggregator;

namespace Thiccdal.Shared.Notifications;
public class TestNotification2 : INotification
{
    public readonly string Message;

    public TestNotification2(string message)
    {
        Message = $"Message type 2: {message}";
    }
}
