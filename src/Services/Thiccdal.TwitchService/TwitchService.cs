using Microsoft.Extensions.Options;
using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.TwitchService.Config;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Thiccdal.TwitchService;

public class TwitchService : IService
{
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
    }



    public async Task Start(CancellationToken cancellationToken)
    {
        ConnectionCredentials credentials = 
            new ConnectionCredentials(_twitchConfig.Login.Username, _twitchConfig.Login.OAuthToken);

        _client.Initialize(credentials);

        _client.OnLog += _client_OnLog;
        _client.OnConnected += _client_OnConnected;
        _client.Connect();
    }

    public async Task Stop()
    {
        _client.Disconnect();
        await Task.CompletedTask;
    }

    private async void _client_OnLog(object? sender, TwitchLib.Client.Events.OnLogArgs e)
    {
        await _eventAggregator.Publish(new RawDataNotification(e.BotUsername, e.DateTime, e.Data), CancellationToken.None);
    }

    private void _client_OnConnected(object? sender, TwitchLib.Client.Events.OnConnectedArgs e)
    {
        foreach (var channel in _twitchConfig.Channels)
        {
            _client.JoinChannel(channel);
            _client.SendMessage(channel, "This is a test");
            
        }

    }
}