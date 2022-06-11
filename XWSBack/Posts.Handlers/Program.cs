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
using Serilog;
using Serilog.Events;
using Shared;

namespace Posts.Handlers
{
    class Program
    {
        static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.WithNsbExceptionDetails().FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/Posts.Handlers.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .UseSerilog()
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

                    services.AddAutoMapper(typeof(Program));
                }).UseNServiceBus(ctx =>
                {
                    var endpointConfig = new EndpointConfiguration(EndpointInstances.PostHandlers);
                    endpointConfig.Configure(EndpointInstances.PostHandlers);

                    return endpointConfig;
                });
        }
    }
}