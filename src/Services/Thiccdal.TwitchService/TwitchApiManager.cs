using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Thiccdal.TwitchService.Config;
using TwitchLib.Api.Core.HttpCallHandlers;

namespace Thiccdal.TwitchService;
internal class TwitchApiManager
{
    private readonly HttpClient _twitchHttpClient;
    private readonly TwitchLib.Api.TwitchAPI _twitchApi;

    private readonly TwitchConfig _twitchConfig;

    public TwitchApiManager(IHttpClientFactory httpClientFactory, IOptions<TwitchConfig> twitchConfig)
    {
        ArgumentNullException.ThrowIfNull(twitchConfig?.Value?.Login?.API, nameof(twitchConfig));
        _twitchConfig = twitchConfig.Value;

        _twitchHttpClient = httpClientFactory.CreateClient("TwitchClient");
        _twitchApi = new TwitchLib.Api.TwitchAPI();
        _twitchApi.Settings.ClientId = _twitchConfig.Login.API.HelixClientId;
        _twitchApi.Settings.AccessToken = _twitchConfig.Login.API.HelixOAuthToken;
    }

    public Task Start(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    // Actions
    public async Task<string> GetAuthToken(string clientId, string clientSecret)
    {
        var msg = new HttpRequestMessage(
            HttpMethod.Post,
            $"oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials"
            );
        var resp = await _twitchHttpClient.PostAsync(
            $"oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials"
            )
    }

    public async Task Shoutout(string channel, string reciever, CancellationToken cancellationToken)
    {
        var msg = new HttpRequestMessage(
        HttpMethod.Post,
        
        );

        string url = $"chat/shoutouts?from_broadcaster_id={channel}&to_broadcaster_id={reciever}&moderator_id={channel}";

        var resp = await _twitchHttpClient.PostAsync("url", null);
    }
}
