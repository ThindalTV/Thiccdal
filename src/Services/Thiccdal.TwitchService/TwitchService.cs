using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Chat;
using Thiccdal.Shared.Notifications.Internals;
using Thiccdal.TwitchService.Config;

using ChatMessage = Thiccdal.Shared.Notifications.Chat.ChatMessage;

namespace Thiccdal.TwitchService;

internal class TwitchService : IService, IEventSubscriber
{
    private CancellationToken _cancellationToken;
    private readonly IEventAggregator _eventAggregator;

    private readonly TwitchChatManager _twitchChatManager;
    private readonly TwitchApiManager _twitchApiManager;

    public TwitchService(
        IEventAggregator eventAggregator,
        TwitchChatManager twitchChatManager,
        TwitchApiManager twitchApiManager)
    {
        _eventAggregator = eventAggregator;
        _twitchChatManager = twitchChatManager;
        _twitchApiManager = twitchApiManager;

        // Setup Twitch Chat
        _twitchChatManager.RecieveMessageHandler = MessageRecievedHandler;

        // Register events to listen for
        _eventAggregator.Subscribe<OutgoingChatMessage>(this, msg => msg.Source.HasFlag(Source.Twitch), SendMessageHandler);
        _eventAggregator.Subscribe<TwitchChatNotification>(this, msg => msg.ChatSource.HasFlag(Source.Twitch) && msg.Message.StartsWith("!so"), ShoutoutHandler);

    }

    private async Task SendMessageHandler(ChatMessage message, CancellationToken cancellationToken)
    {
        if (await _twitchChatManager.InChannel(message.Channel))
        {
            await _twitchChatManager.SendMessage(message.Channel, message.Message, cancellationToken);
            await _eventAggregator.Publish(new LogMessageNotification(nameof(TwitchService), $"Message to {message.Channel}: {message.Message}"));
        }
    }

    private async Task MessageRecievedHandler(TwitchChatNotification msg)
    {
        await _eventAggregator.Publish(msg, cancellationToken: _cancellationToken);
    }

    private async Task ShoutoutHandler(TwitchChatNotification inc, CancellationToken ct)
    {
        var parts = inc.Message.Split(' ');
        if (parts.Length < 1)
            return;
        var recipient = inc.Message.Split(' ')[1];
        await _twitchApiManager.Shoutout(inc.Channel, recipient, ct);
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;

        await _twitchChatManager.Start(cancellationToken);
        //await _twitchApiManager.Start(cancellationToken);

        await _eventAggregator.Publish(new OutgoingChatMessage(Source.Twitch, "Thindal", "thiccdal", DateTime.Now, "Thiccdal is now online!"));
    }

    public async Task Stop()
    {
        await _twitchChatManager.Stop(_cancellationToken);
        await _twitchApiManager.Stop(_cancellationToken);
        await Task.CompletedTask;
    }



    private bool _disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _eventAggregator.Unsubscribe<Shared.Notifications.Chat.ChatMessage>(this);
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}