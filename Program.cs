using Azure.Data.Tables;
using Azure.Storage.Queues;
using CoffeeNChill.Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        var storageConnectionString =
            Environment.GetEnvironmentVariable("AzureWebJobsStorage")
            ?? "UseDevelopmentStorage=true";

        // Table Storage — shared singleton client
        services.AddSingleton(new TableServiceClient(storageConnectionString));

        // Queue Storage — shared singleton client
        services.AddSingleton(new QueueServiceClient(storageConnectionString));

        // Singleton (not Scoped) — Queue-triggered functions are singletons
        services.AddSingleton<OrderRepository>();
    })
    .Build();

host.Run();