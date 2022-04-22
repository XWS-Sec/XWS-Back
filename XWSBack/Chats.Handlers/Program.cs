using System;
using Chats.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using NServiceBus;
using Shared;
using EndpointInstances = Shared.EndpointInstances;

namespace Chats.Handlers
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
                })
                .ConfigureServices(services =>
                {
                    var rabbitMq = Environment.GetEnvironmentVariable("USE_RMQ");
                    if (!string.IsNullOrEmpty(rabbitMq))
                    {
                        var rabbitMqHostname = Environment.GetEnvironmentVariable("RMQ_HOST") ?? "rabbitmq";
                        services.AddSingleton<IHostedService>(new ProceedIfRabbitMqIsAlive(rabbitMqHostname));
                    }

                    services.AddSingleton<IMongoClient, MongoClient>(s => SetupMongoDb.CreateClient<Chat>("ChatsHandlers", "Chats"));
                    BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));
                }).UseNServiceBus(ctx =>
                {
                    var endpointConfig = new EndpointConfiguration(EndpointInstances.ChatHandlers);
                    var routing = endpointConfig.Configure(EndpointInstances.ChatHandlers);

                    return endpointConfig;
                });
        }
    }
}