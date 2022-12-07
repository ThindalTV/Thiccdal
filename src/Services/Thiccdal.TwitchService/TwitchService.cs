using Thiccdal.Interfaces;

namespace Thiccdal.TwitchService;

public class TwitchService : IService
{
    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("TwitchService is running.");
            try
            {
                await Task.Delay(1000, cancellationToken);
            } catch(TaskCanceledException)
            {
                // NOOP, expected upon cancellation
            }
        }
    }

    public async Task Stop()
    {
        // NOOP
        await Task.CompletedTask;
    }
}