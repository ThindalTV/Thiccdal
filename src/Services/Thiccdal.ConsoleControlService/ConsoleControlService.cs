using Thiccdal.Interfaces;

namespace Thiccdal.ConsoleControlService;

public class ConsoleControlService : IService
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
}