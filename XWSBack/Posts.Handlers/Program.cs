using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using NServiceBus;
using Posts.Model;
using Shared;

namespace Posts.Handlers
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
                    services.AddSingleton<IMongoClient, MongoClient>(s => SetupMongoDb.CreateClient<Post>("PostsHandlers", "Posts"));
                    BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));
                    
                    var serviceInstances = typeof(Program).Assembly.GetTypes()
                        .Where(x => x.Namespace.EndsWith("Services") && x.Name.EndsWith("Service"));

                    foreach (var serviceInstance in serviceInstances)
                    {
                        services.AddSingleton(serviceInstance);
                    }
                }).UseNServiceBus(ctx =>
                {
                    var endpointConfig = new EndpointConfiguration(EndpointInstances.PostHandlers);
                    var routing = endpointConfig.Configure(EndpointInstances.PostHandlers);

                    return endpointConfig;
                });
        }
    }
}