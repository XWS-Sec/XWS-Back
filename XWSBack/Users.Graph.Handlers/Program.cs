using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using NServiceBus;
using Serilog;
using Serilog.Events;
using Shared;
using Users.Graph.Handlers.Services;

namespace Users.Graph.Handlers
{
    class Program
    {
        static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.WithNsbExceptionDetails().FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/BaseApi.txt", rollingInterval: RollingInterval.Day)
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
                    var uri = Environment.GetEnvironmentVariable("NEO4J_URI") ?? "bolt://localhost:7687";
                    var user = Environment.GetEnvironmentVariable("NEO4J_USER") ?? "neo4j";
                    var pass = Environment.GetEnvironmentVariable("NEO4J_PASS") ?? "neo4j";
                    var graphClient = new BoltGraphClient(new Uri(uri), user, pass);
                    graphClient.ConnectAsync();

                    services.AddSingleton<IGraphClient, BoltGraphClient>(s => graphClient);

                    var serviceInstances = typeof(Program).Assembly.GetTypes()
                        .Where(x => x.Namespace.EndsWith("Services") && x.Name.EndsWith("Service"));

                    foreach (var serviceInstance in serviceInstances)
                    {
                        services.AddSingleton(serviceInstance);
                    }

                }).UseNServiceBus(ctx =>
                {
                    var endpointConfig = new EndpointConfiguration(EndpointInstances.UserGraphHandlers);
                    endpointConfig.Configure(EndpointInstances.UserGraphHandlers);

                    return endpointConfig;
                });
        }
    }
}