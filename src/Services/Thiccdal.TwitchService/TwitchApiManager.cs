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
using System.Text.Json;
using System.Threading;

namespace Thiccdal.TwitchService;
internal class TwitchApiManager
{
    private readonly HttpClient _twitchHttpClient;
    private readonly HttpClient _httpClient;

    private readonly TwitchLib.Api.TwitchAPI _twitchApi;

    private readonly TwitchConfig _twitchConfig;


    public TwitchApiManager(IHttpClientFactory httpClientFactory, IOptions<TwitchConfig> twitchConfig)
    {
        ArgumentNullException.ThrowIfNull(twitchConfig?.Value?.Login?.API, nameof(twitchConfig));
        _twitchConfig = twitchConfig.Value;

        _twitchHttpClient = httpClientFactory.CreateClient("TwitchClient");
        _httpClient = httpClientFactory.CreateClient();

        _twitchApi = new TwitchLib.Api.TwitchAPI();
        _twitchApi.Settings.ClientId = _twitchConfig.Login.API.HelixClientId;
        _twitchApi.Settings.AccessToken = _twitchConfig.Login.API.HelixOAuthToken;
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        await GetAuthToken(_twitchConfig.Login.API.HelixClientId, _twitchConfig.Login.API.HelixClientSecret, cancellationToken);
    }

    public async Task Stop(CancellationToken cancellationToken)
    {
        // NOOP
    }

    // Actions
    public async Task GetAuthToken(string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        var msg = new HttpRequestMessage(
            HttpMethod.Post,
            $"oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials"
            );
        var resp = await _httpClient.SendAsync(msg, cancellationToken);

        if (!resp.IsSuccessStatusCode || resp.Content == null)
        {
            throw new InvalidOperationException("Failed to get auth token");
        }

        var serializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        dynamic tokenObject = (await JsonSerializer.DeserializeAsync<object>(
            resp.Content.ReadAsStream(),
            serializerOptions)) ?? new object();
        _twitchHttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenObject.access_token}");
    }

    public async Task Shoutout(string channel, string reciever, CancellationToken cancellationToken)
    {
        var msg = new HttpRequestMessage(
            HttpMethod.Post,
            $"chat/shoutouts?from_broadcaster_id={_twitchConfig.Login.API.HelixClientId}&to_broadcaster_id={reciever}&moderator_id={channel}"
        );

        var resp = await SendPostMessage(msg, cancellationToken);

        if (!resp.IsSuccessStatusCode || resp.Content == null)
        {
            throw new InvalidOperationException($"Failed to shoutout. StatusCode {resp.StatusCode}");
        }
    }

    public async Task<HttpResponseMessage?> SendPostMessage(HttpRequestMessage message, CancellationToken cancellationToken)
    {
        var response = await _twitchHttpClient.SendAsync(message, cancellationToken);

        if(cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        if( response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await GetAuthToken(_twitchConfig.Login.API.HelixClientId, _twitchConfig.Login.API.HelixClientSecret, cancellationToken);
            response = await _twitchHttpClient.SendAsync(message);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        if ( !response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to send twitch request. Status code {response.StatusCode}");
        }

        return await _twitchHttpClient.SendAsync(message);
    }
}
