using Thiccdal.Shared;

namespace Thiccdal.ConsoleControlService;

public class ConsoleControlService : IService
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private int _counter;
    public ConsoleControlService(CancellationTokenSource cancellationTokenSource)
    {
        _counter = 0;
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
}