using System;
using System.Linq;
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
                    var serviceInstances = typeof(Program).Assembly.GetTypes()
                        .Where(x => x.Namespace.EndsWith("Services") && x.Name.EndsWith("Service"));

                    foreach (var serviceInstance in serviceInstances)
                    {
                        services.AddSingleton(serviceInstance);
                    }
                    
                    services.AddSingleton<IMongoClient, MongoClient>(s => SetupMongoDb.CreateClient<Chat>("ChatsHandlers", "Chats"));
                    BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));

                    services.AddAutoMapper(typeof(Program));
                }).UseNServiceBus(ctx =>
                {
                    var endpointConfig = new EndpointConfiguration(EndpointInstances.ChatHandlers);
                    var routing = endpointConfig.Configure(EndpointInstances.ChatHandlers);

                    return endpointConfig;
                });
        }
    }
}