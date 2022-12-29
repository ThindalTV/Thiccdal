using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;

namespace Thiccdal.ConsoleControlService;

public class ConsoleControlService : IService, IEventSubscriber
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly IEventAggregator _eventAggregator;

    public ConsoleControlService(CancellationTokenSource cancellationTokenSource, IEventAggregator eventAggregator)
    {
        _cancellationTokenSource = cancellationTokenSource;
        _eventAggregator = eventAggregator;
        _eventAggregator.Subscribe<RawDataNotification>(this, RawDataNotificationHandler);
    }

    private async Task RawDataNotificationHandler(RawDataNotification notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"RawDataNotification: {notification.DateTime.ToShortTimeString()} - {notification.Context}: {notification.RawData}");
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        // NOOP
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