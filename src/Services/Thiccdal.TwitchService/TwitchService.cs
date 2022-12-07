using MediatR;
using Thiccdal.Shared;
using Thiccdal.Shared.Contracts;

namespace Thiccdal.TwitchService;

public class TwitchService : IService
{
    IMediator _mediator;
    public TwitchService(IMediator mediator)
    {
        _mediator = mediator;
    }
        
    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("TwitchService is running.");
            await _mediator.Publish(new TestRequest() { Message = "Hello from TwitchService" }, cancellationToken);
            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch (TaskCanceledException)
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