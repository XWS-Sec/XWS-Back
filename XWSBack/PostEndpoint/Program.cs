using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NServiceBus;
using PostServiceModel;
using Shared;

namespace PostService
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                }).ConfigureServices(services =>
                {
                    var rabbitMq = Environment.GetEnvironmentVariable("USE_RMQ");
                    if (!string.IsNullOrEmpty(rabbitMq))
                    {
                        var rabbitMqHostname = Environment.GetEnvironmentVariable("RMQ_HOST") ?? "rabbitmq";
                        services.AddSingleton<IHostedService>(new ProceedIfRabbitMqIsAlive(rabbitMqHostname));
                    }
                    
                    services.AddSingleton<IMongoClient, MongoClient>(s => SetupMongoDb.CreateClient<Post>("Post", "Posts"));
                    
                }).UseNServiceBus(ctx =>
                {
                    var endpointConfig = new EndpointConfiguration(EndpointInstances.PostEndpoint);
                    
                    var routing = endpointConfig.Configure(EndpointInstances.PostEndpoint);
                    /* Mapping of messages */
                    
                    return endpointConfig;
                });
        }
    }
}