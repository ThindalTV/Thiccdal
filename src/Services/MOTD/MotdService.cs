using System.Threading;
using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Chat;
using Thiccdal.Shared.Repositories;

namespace MOTD;

public class MotdService : IService, IEventSubscriber
{
    private readonly IEventAggregator _eventAggregator;
    private bool disposedValue;
    private readonly IRepository _repository;

    private readonly string _repoName = "Motd";

    public MotdService(IEventAggregator eventAggregator, IRepository repository)
    {
        _repository = repository;
        _eventAggregator = eventAggregator;
        _eventAggregator.Subscribe<JoinedChannelMessage>(this, HandleJoinedChannel);
        _eventAggregator.Subscribe<TwitchChatNotification>(this, (msg) => msg.Message.StartsWith("!motd"), HandleMotdCommand);

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
        var msg = await GetRandomMessage(joinedChannelMessage.ChannelName, cancellationToken);
        if(msg != null)
            await _eventAggregator.Publish(new OutgoingChatMessage(joinedChannelMessage.Source, joinedChannelMessage.ChannelName, "Thiccdal", DateTime.Now, msg), this, cancellationToken);
    }

    private async Task HandleMotdCommand(TwitchChatNotification chatMessage, CancellationToken cancellationToken)
    {
        var dukas = new[] { "dukasoft", "thindal" };
        var dukaName = chatMessage.User.Name.ToLower();
        if (!dukas.Contains(dukaName))
            return;

        var parts = chatMessage.Message.Split(' ');
        if (parts.Length < 2)
            return;

        int prefixLength = parts[0].Length + parts[1].Length + 2;

        var channelMessages = await _repository.Get<List<string>>(_repoName, chatMessage.Channel, cancellationToken)
            ?? new List<string>();

        switch (parts[1].ToLower())
        {
            case "add":
                if (parts.Length < 3)
                    return;
                channelMessages.Add(chatMessage.Message.Substring(prefixLength));
                await _repository.Set(_repoName, chatMessage.Channel, channelMessages, cancellationToken);
                await _eventAggregator.Publish(new OutgoingChatMessage(chatMessage.ChatSource, chatMessage.Channel, "Thiccdal", DateTime.Now, "Added message"), this, cancellationToken);
                break;
            case "remove":
                if (parts.Length < 3)
                    return;
                channelMessages.Remove(chatMessage.Message.Substring(prefixLength));
                await _repository.Set(_repoName, chatMessage.Channel, channelMessages, cancellationToken);
                await _eventAggregator.Publish(new OutgoingChatMessage(chatMessage.ChatSource, chatMessage.Channel, "Thiccdal", DateTime.Now, "Removed message"), this, cancellationToken);
                break;
            case "list":
                foreach( var message in channelMessages)
                {
                    await _eventAggregator.Publish(new OutgoingChatMessage(chatMessage.ChatSource, chatMessage.Channel, "Thiccdal", DateTime.Now, message), this, cancellationToken);

                }
                break;
        }
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
    private async Task<string?> GetRandomMessage(string channelName, CancellationToken cancellationToken)
    {
        var messages = await _repository.Get<List<string>>(_repoName, channelName, cancellationToken);
        if (messages == null || messages.Count == 0)
            return null;

        var random = new Random();
        var index = random.Next(messages.Count);

        return messages[index];
    }
}
