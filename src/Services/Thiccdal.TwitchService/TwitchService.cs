using Microsoft.Extensions.Options;
using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Internals;
using Thiccdal.TwitchService.Config;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

using ChatMessage = Thiccdal.Shared.Notifications.Chat.ChatMessage;

namespace Thiccdal.TwitchService;

public class TwitchService : IService, IEventSubscriber
{
    private CancellationToken _cancellationToken;
    private bool disposedValue;
    private readonly IEventAggregator _eventAggregator;
    private readonly TwitchConfig _twitchConfig;
    private readonly TwitchClient _client;
    public TwitchService(IEventAggregator eventAggregator, IOptions<TwitchConfig> twitchConfig)
    {
        _eventAggregator = eventAggregator;
        _twitchConfig = twitchConfig?.Value ?? throw new ArgumentNullException(nameof(twitchConfig));
        _ = _twitchConfig.Login ?? throw new ArgumentNullException(nameof(twitchConfig), "Missing login information.");

        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };

        WebSocketClient customClient = new WebSocketClient(clientOptions);
        _client = new TwitchClient(customClient);

        // Register events to listen for
        _eventAggregator.Subscribe<ChatMessage>(this, SendMessageHandler);
    }

    private async Task SendMessageHandler(ChatMessage message, CancellationToken cancellationToken)
    {
        if(message.Source != Source.Twitch.ToString())
        {
            return;
        }

        if( _client.JoinedChannels.Any(js => js.Channel.ToLower() == message.Channel.ToLower()) )
        {
            _client.SendMessage(message.Channel, message.Message);
            await _eventAggregator.Publish(new LogMessageNotification(nameof(TwitchService), $"Message to {message.Channel}: {message.Message}"));
        }
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        ConnectionCredentials credentials = 
            new ConnectionCredentials(_twitchConfig.Login.Username, _twitchConfig.Login.OAuthToken);

        _client.Initialize(credentials);

        _client.OnLog += _client_OnLog;
        _client.OnConnected += _client_OnConnected;
        _client.OnMessageReceived += _client_OnMessageReceived;
        _client.Connect();
    }



    public async Task Stop()
    {
        _client.Disconnect();
        await Task.CompletedTask;
    }

    private async void _client_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        long unixTimeTicks = long.Parse(e.ChatMessage.TmiSentTs);
        DateTime messageTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local).AddTicks(unixTimeTicks * TimeSpan.TicksPerMillisecond);

        await _eventAggregator.Publish(new ChatMessage(Source.Twitch.ToString(), e.ChatMessage.Channel, e.ChatMessage.Username, messageTime, e.ChatMessage.Message), this, _cancellationToken);
        // TODO: Attempt to locate user object. If properties have changed update it
        var user = new UserInfo()
        {
            UserSource = Source.Twitch,
            Id = 1,
            Name = e.ChatMessage.Username,
            SubscribedForMonths = e.ChatMessage.SubscribedMonthCount,
            Flags = UserFlags.None
        };

        if( e.ChatMessage.IsSubscriber)
        {
            user.Flags |= UserFlags.Subscriber;
        }
        if(e.ChatMessage.IsVip)
        {
            user.Flags |= UserFlags.Vip;
        }
        if(e.ChatMessage.IsPartner)
        {
            user.Flags |= UserFlags.TwitchPartner;
        }
        if(e.ChatMessage.IsModerator)
        {
            user.Flags |= UserFlags.Moderator;
        }
        if(e.ChatMessage.IsStaff)
        {
            user.Flags |= UserFlags.TwitchStaff;
        }
        if(e.ChatMessage.IsBroadcaster)
        {
            user.Flags |= UserFlags.TwitchBroadcaster;
        }

        MessageFlags messageFlags = MessageFlags.None;
        if (e.ChatMessage.IsFirstMessage)
        {
            messageFlags |= MessageFlags.UsersFirstMessage;
        }
        if( e.ChatMessage.IsHighlighted)
        {
            messageFlags |= MessageFlags.Highlighted;
        }
        if(e.ChatMessage.Bits > 0)
        {
            messageFlags |= MessageFlags.Donation;
        }

        var msg = new TwitchChatNotification
        {
            ChatSource = Source.Twitch,
            MessageId = e.ChatMessage.Id,
            Datetime = messageTime,
            Channel = e.ChatMessage.Channel,
            User = user,
            Message = e.ChatMessage.Message,
            Bits = e.ChatMessage.Bits,
            BitsInDollars = (decimal)e.ChatMessage.BitsInDollars,
            Flags = messageFlags
        };

        await _eventAggregator.Publish(msg, cancellationToken: _cancellationToken);
    }

    private async void _client_OnLog(object? sender, TwitchLib.Client.Events.OnLogArgs e)
    {
        await _eventAggregator.Publish(new RawData(e.BotUsername, e.DateTime, e.Data), cancellationToken: _cancellationToken);
    }

    private void _client_OnConnected(object? sender, TwitchLib.Client.Events.OnConnectedArgs e)
    {
        foreach (var channel in _twitchConfig.Channels)
        {
            _client.JoinChannel(channel);
            // Lets not send a message
            //_client.SendMessage(channel, "Hi everyone! I am a bot and @thindal is my daddy. I'll be quiet now. <3");
            
        }

    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _eventAggregator.Unsubscribe<Shared.Notifications.Chat.ChatMessage>(this);
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}