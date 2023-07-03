using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Thiccdal.Shared;

namespace Thiccdal.WebSocketHost;
public class WebSocketHostService : IService
{
    public static void Main()
    {
        // Dummy method to satisfy the compiler
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        var hostBuilder = WebHost.CreateDefaultBuilder()
            .ConfigureKestrel(options =>
            {
                options.ListenAnyIP(1234);

            })
            .ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSignalR();
            })
            .Configure(appBuilder =>
            {
                appBuilder.UseRouting();

                appBuilder.UseEndpoints(endPoints =>
                {
                    endPoints.MapHub<MyHub>(MyHub.HubUrl);
                });
            });

        await hostBuilder
            .Build()
            .RunAsync(cancellationToken);
    }

    public async Task Stop()
    {
        await Task.CompletedTask;
    }
}

class MyHub : Hub
{
    public const string HubUrl = "/hub";

    public async Task Send(string name, string message)
    {
        await Clients.All.SendAsync("Send", name, message);
    }
}

