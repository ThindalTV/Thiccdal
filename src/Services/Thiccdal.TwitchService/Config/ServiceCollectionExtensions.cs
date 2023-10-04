using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Thiccdal.Shared;

namespace Thiccdal.TwitchService.Config;
public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddTwitchService(this ServiceCollection servicesCollection, IConfiguration config)
    {
        servicesCollection.AddHttpClient("TwitchClient", (opt) =>
        {
            opt.BaseAddress = new Uri("https://api.twitch.tv/helix/");
            opt.DefaultRequestHeaders.Add("Client-Id", config["Twitch:Login:API:HelixClientId"]);

            opt.DefaultRequestHeaders.Add("Authorization", $"Bearer {config["Twitch:Login:API:HelixOAuthToken"]}");
        });

        servicesCollection.AddSingleton<IService, TwitchService>();

        // Internal services
        servicesCollection.AddSingleton<TwitchChatManager, TwitchChatManager>();
        servicesCollection.AddSingleton<TwitchApiManager, TwitchApiManager>();

        // Config
        var twitchConfigSection = config.GetSection(TwitchConfig.SectionName);
        servicesCollection.Configure<TwitchConfig>(twitchConfigSection);

        // Do I really need these? Maybe?
        servicesCollection.Configure<TwitchAPI>(twitchConfigSection.GetSection(TwitchAPI.SectionName));
        servicesCollection.Configure<TwitchIRC>(config.GetSection(TwitchIRC.SectionName));

        return servicesCollection;
    }
}
