﻿using Microsoft.Extensions.DependencyInjection;
using Thiccdal.Interfaces;
using Thiccdal.TwitchService;
using Thiccdal.ConsoleControlService;

Console.WriteLine("Starting Thiccdal.");

CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

var servicesCollection = new ServiceCollection();
servicesCollection.AddSingleton((IServiceProvider) => cancellationTokenSource);
// Register services to DI
Console.WriteLine("Registering services.");

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