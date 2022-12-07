using MediatR;
using Thiccdal.Shared;
using Thiccdal.Shared.Contracts;

namespace Thiccdal.ConsoleControlService;

public class ConsoleControlService : IService, INotificationHandler<TestRequest>
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    public ConsoleControlService(CancellationTokenSource cancellationTokenSource)
    {
        _cancellationTokenSource = cancellationTokenSource;
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

    Task INotificationHandler<TestRequest>.Handle(TestRequest notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"{nameof(ConsoleControlService)} recieved notification: {notification.Message}");
        return Task.CompletedTask;
    }
}