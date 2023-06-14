using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Internals;

namespace Thiccdal.ConsoleControlService;

public class ConsoleControlService : IService, IEventSubscriber
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly IEventAggregator _eventAggregator;

    public ConsoleControlService(CancellationTokenSource cancellationTokenSource, IEventAggregator eventAggregator)
    {
        _cancellationTokenSource = cancellationTokenSource;
        _eventAggregator = eventAggregator;
        _eventAggregator.Subscribe<RawData>(this, RawDataNotificationHandler);
        _eventAggregator.Subscribe<LogMessageNotification>(this, LogMessageNotificationHandler);
    }

    private async Task LogMessageNotificationHandler(LogMessageNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{notification.Timestamp.ToShortTimeString()} - {notification.Sender}: {notification.Message}");
        await Task.CompletedTask;
    }

    private async Task RawDataNotificationHandler(RawData notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"RawDataNotification: {notification.DateTime.ToShortTimeString()} - {notification.Context}: {notification.Data}");
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<RawData>(this);
        _eventAggregator.Unsubscribe<LogMessageNotification>(this);
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            Console.ReadLine();
            _cancellationTokenSource.Cancel();
        }, cancellationToken);
    }

    public Task Stop() => Task.CompletedTask;
}