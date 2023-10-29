using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications.Chat;

namespace MOTD;

public class MotdService : IService, IEventSubscriber
{
    private readonly IEventAggregator _eventAggregator;
    private bool disposedValue;

    public MotdService(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _eventAggregator.Subscribe<JoinedChannelMessage>(this, HandleJoinedChannel);
    }

    public Task Start(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    private async Task HandleJoinedChannel(JoinedChannelMessage joinedChannelMessage, CancellationToken cancellationToken)
    {
        string msg = $"Hello! I'm Thiccdal, a bot created by Thindal. I'm here to do things and mess them up.";
        await _eventAggregator.Publish(new OutgoingChatMessage(joinedChannelMessage.Source, joinedChannelMessage.ChannelName, "Thiccdal", DateTime.Now, msg), this, cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~MotdService()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
