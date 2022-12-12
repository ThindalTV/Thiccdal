using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;

namespace Thiccdal.TwitchService;

public class TwitchService : IService
{
    IEventAggregator _eventAggregator;
    public TwitchService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
    }
    public async Task Start(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, cancellationToken);
                await _eventAggregator.Publish(new TestNotification1("From TwitchService"), cancellationToken);
                await _eventAggregator.Publish(new TestNotification2("From TwitchService"), cancellationToken);
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