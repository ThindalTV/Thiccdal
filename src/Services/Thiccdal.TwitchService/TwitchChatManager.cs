using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Chat;
using Thiccdal.Shared.Notifications.Internals;
using Thiccdal.TwitchService.Config;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Thiccdal.TwitchService;
internal class TwitchChatManager
{
    private readonly TwitchClient _client;
    private readonly TwitchConfig _twitchConfig;

    public Func<TwitchChatNotification, Task>? RecieveMessageHandler;

    public string[] JoinedChannels => _client.JoinedChannels.Select(js => js.Channel).ToArray();

    public TwitchChatManager(IOptions<TwitchConfig> twitchConfig)
    {
        ArgumentNullException.ThrowIfNull(twitchConfig, nameof(twitchConfig));
        ArgumentNullException.ThrowIfNull(twitchConfig.Value, nameof(twitchConfig.Value));
        _twitchConfig = twitchConfig.Value ?? throw new ArgumentNullException(nameof(twitchConfig));

        ArgumentNullException.ThrowIfNull(_twitchConfig.Channels, nameof(_twitchConfig.Channels));
        ArgumentNullException.ThrowIfNull(_twitchConfig.Login, nameof(_twitchConfig.Login));
        ArgumentNullException.ThrowIfNull(_twitchConfig.Login.IRC, nameof(_twitchConfig.Login.IRC));
        ArgumentNullException.ThrowIfNull(_twitchConfig.Login.API, nameof(_twitchConfig.Login.IRC.Username));

        // Setup client
        var webSocketOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        WebSocketClient socketClient = new WebSocketClient(webSocketOptions);

        _client = new TwitchClient(socketClient);
        var credentials = new ConnectionCredentials(
            _twitchConfig.Login.IRC.Username,
            _twitchConfig.Login.IRC.OAuthToken);

        _client.Initialize(credentials);

        _client.OnConnected += ClientOnConnected;
        _client.OnMessageReceived += ClientOnMessageReceived;
    }

    public Task<bool> InChannel(string channelName)
    {
        return Task.FromResult(JoinedChannels.Any(js => string.Equals(
            js,
            channelName,
            StringComparison.CurrentCultureIgnoreCase)));
    }

    public Task SendMessage(string channel, string message, CancellationToken cancellationToken)
    {
        _client.SendMessage(channel, message);
        return Task.CompletedTask;
    }

    private void ClientOnConnected(object? sender, OnConnectedArgs e)
    {
        // Join channels
        if (_twitchConfig.Channels != null)
        {
            foreach (var channel in _twitchConfig.Channels)
            {
                _client.JoinChannel(channel);
            }
        }
    }

    private void ClientOnMessageReceived(object? sender, OnMessageReceivedArgs e)
    {
        // TODO: Setup message
        long unixTimeTicks = long.Parse(e.ChatMessage.TmiSentTs);
        DateTime messageTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddTicks(unixTimeTicks * TimeSpan.TicksPerMillisecond);

        var flags = GetFlags(e.ChatMessage);

        var user = new UserInfo()
        {
            UserSource = Source.Twitch,
            Id = 1,
            Name = e.ChatMessage.Username,
            SubscribedForMonths = e.ChatMessage.SubscribedMonthCount,
            Flags = flags.userFlags
        };


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
            Flags = flags.messageFlags
        };

        RecieveMessageHandler?.Invoke(msg);
    }

    public async Task Start(CancellationToken cancellationToken)
    {

        _client.Connect();

        await Task.CompletedTask;
        return;
    }

    public async Task Stop(CancellationToken cancellationToken)
    {
        _client.Disconnect();
        await Task.CompletedTask;
    }
    internal (MessageFlags messageFlags, UserFlags userFlags) GetFlags(TwitchLib.Client.Models.ChatMessage chatMessage)
    {
        UserFlags userFlags = UserFlags.None;
        if (chatMessage.IsSubscriber)
        {
            userFlags |= UserFlags.Subscriber;
        }
        if (chatMessage.IsVip)
        {
            userFlags |= UserFlags.Vip;
        }
        if (chatMessage.IsPartner)
        {
            userFlags |= UserFlags.TwitchPartner;
        }
        if (chatMessage.IsModerator)
        {
            userFlags |= UserFlags.Moderator;
        }
        if (chatMessage.IsStaff)
        {
            userFlags |= UserFlags.TwitchStaff;
        }
        if (chatMessage.IsBroadcaster)
        {
            userFlags |= UserFlags.TwitchBroadcaster;
        }

        MessageFlags messageFlags = MessageFlags.None;
        if (chatMessage.IsFirstMessage)
        {
            messageFlags |= MessageFlags.UsersFirstMessage;
        }
        if (chatMessage.IsHighlighted)
        {
            messageFlags |= MessageFlags.Highlighted;
        }
        if (chatMessage.Bits > 0)
        {
            messageFlags |= MessageFlags.Donation;
        }

        return (messageFlags, userFlags);
    }
}

