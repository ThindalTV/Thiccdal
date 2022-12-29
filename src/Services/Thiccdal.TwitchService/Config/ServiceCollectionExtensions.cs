using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Thiccdal.Shared;

namespace Thiccdal.TwitchService.Config;
public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddTwitchService(this ServiceCollection servicesCollection, IConfiguration config)
    {
        servicesCollection.AddSingleton<IService, TwitchService>();
        servicesCollection.Configure<TwitchConfig>(config.GetSection(TwitchConfig.SectionName));

        return servicesCollection;
    }
}
