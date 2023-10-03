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
            opt.DefaultRequestHeaders.Add("Client-Id", config["Twitch:Login:Client-Id"]);

            opt.DefaultRequestHeaders.Add("Authorization", $"Bearer {config["Twitch:Login:Client-Secret"]}");
        });

        servicesCollection.AddSingleton<IService, TwitchService>();
        servicesCollection.Configure<TwitchConfig>(config.GetSection(TwitchConfig.SectionName));

        return servicesCollection;
    }
}
