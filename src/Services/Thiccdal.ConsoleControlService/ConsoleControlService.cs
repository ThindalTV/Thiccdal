using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;

namespace Thiccdal.ConsoleControlService;

public class ConsoleControlService : IService, IEventSubscriber
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly IEventAggregator _eventAggregator;

    private int _counter;
    public ConsoleControlService(CancellationTokenSource cancellationTokenSource, IEventAggregator eventAggregator)
    {
        _counter = 0;
        _cancellationTokenSource = cancellationTokenSource;
        _eventAggregator = eventAggregator;
        _eventAggregator.Subscribe<TestNotification1>(this, TestNotification1Handler);
        _eventAggregator.Subscribe<TestNotification2>(this, TestNotification2Handler);
    }

    public void Dispose()
    {
        // NOOP
    }

    private async Task TestNotification1Handler(TestNotification1 message, CancellationToken token)
    {
        Console.WriteLine($"ConsoleControlService recieved TestNotification1: {message.Message}");
    }

    private async Task TestNotification2Handler(TestNotification2 message, CancellationToken token)
    {
        Console.WriteLine($"ConsoleControlService recieved TestNotification2: {message.Message}");
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            Console.ReadLine();
            _cancellationTokenSource.Cancel();
        }, cancellationToken);
    }

    public Task Stop()
    {
        // NOOP
        return Task.CompletedTask;
    }
}