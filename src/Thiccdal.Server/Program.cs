using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ResponderService;
using System.Reflection;
using Thiccdal.ConsoleControlService;
using Thiccdal.EventAggregator;
using Thiccdal.OverlayService;
using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.TwitchService.Config;
using Thiccdal.WebSocketHost;
using TwitchShoutoutService;

Console.WriteLine("Starting Thiccdal.");

// Reading config
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .Build();


CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

var servicesCollection = new ServiceCollection();
servicesCollection.AddSingleton((IServiceProvider) => cancellationTokenSource);
servicesCollection.AddLogging(config => config.AddConsole());


servicesCollection.AddSingleton<IEventAggregator, EventAggregator>();
// Register services to DI
Console.WriteLine("Registering Thiccdal services.");
servicesCollection.AddTwitchService(config);
servicesCollection.AddSingleton<IService, ConsoleControlService>();
servicesCollection.AddSingleton<IService, OverlayConnectionService>();
servicesCollection.AddSingleton<IService, WebSocketHostService>();
servicesCollection.AddSingleton<IService, ChatResponderService>();
servicesCollection.AddSingleton<IService, ShoutoutService>();

var serviceProvider = servicesCollection.BuildServiceProvider();

// Load services
Console.WriteLine("Loading services.");
var services = serviceProvider.GetServices<IService>();

// Start services
Console.WriteLine("Starting services.");

var serviceTasks = new List<Task>();
foreach (var service in services)
{
    Console.WriteLine($"{service.GetType().Name} is starting.");
    serviceTasks.Add(service.Start(cancellationTokenSource.Token));
}

// Wait for services to stop
Console.WriteLine("Thiccdal services are running.");
await Task.WhenAll(serviceTasks);
Console.WriteLine("Thiccdal services has requested to stop.");

serviceTasks.Clear();

// Stop services
foreach(var service in services)
{
    Console.WriteLine($"{service.GetType().Name} is stopping.");
    serviceTasks.Add(service.Stop());
}

await Task.WhenAll(serviceTasks);
Console.WriteLine("Thiccdal has stopped.");