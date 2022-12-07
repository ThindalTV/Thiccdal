using Microsoft.Extensions.DependencyInjection;
using Thiccdal.TwitchService;
using Thiccdal.ConsoleControlService;
using MediatR;
using System.Reflection;
using Thiccdal.Shared;

Console.WriteLine("Starting Thiccdal.");

CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

var servicesCollection = new ServiceCollection();
servicesCollection.AddSingleton((IServiceProvider) => cancellationTokenSource);

Console.WriteLine("Registering Mediatr.");
servicesCollection.AddMediatR(GetThiccdalAssemblies().ToArray());

// Register services to DI
Console.WriteLine("Registering Thiccdal services.");

servicesCollection.AddSingleton<IService, ConsoleControlService>();
servicesCollection.AddSingleton<IService, TwitchService>();

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

IEnumerable<Assembly> GetThiccdalAssemblies()
{
    var list = new List<string>();
    var stack = new Stack<Assembly>();

    stack.Push(Assembly.GetEntryAssembly() ?? throw new NullReferenceException("Failed to locate entry assembly. Huh?"));

    do
    {
        var asm = stack.Pop();

        yield return asm;

        foreach (var reference in asm.GetReferencedAssemblies())
            if (!list.Contains(reference.FullName) && reference.FullName.ToLower().StartsWith("thiccdal"))
            {
                stack.Push(Assembly.Load(reference));
                list.Add(reference.FullName);
            }

    }
    while (stack.Count > 0);

}