using System;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Serilog;
using Serilog;
using Shared.Custom;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Shared
{
    public static class NServiceBusConfiguration
    {
        public static RoutingSettings Configure(this EndpointConfiguration endpointConfig, string endpointName)
        {
            endpointConfig.AuditProcessedMessagesTo("audit");
            endpointConfig.SendFailedMessagesTo("error");
            endpointConfig.SendHeartbeatTo("Particular.ServiceControl", TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(15));
            var metrics = endpointConfig.EnableMetrics();

            metrics.SendMetricDataToServiceControl("Particular.Monitoring", TimeSpan.FromSeconds(3));

            var databus = endpointConfig.UseDataBus<FileShareDataBus>();
            databus.BasePath(Environment.GetEnvironmentVariable("DATABUS_PATH") ?? "C:\\NServiceBusFileShare");

            var conventions = endpointConfig.Conventions();
            conventions.DefiningCommandsAs(t => typeof(ICustomCommand).IsAssignableFrom(t));
            conventions.DefiningEventsAs(t => typeof(ICustomEvent).IsAssignableFrom(t));
            conventions.DefiningMessagesAs(t => typeof(ICustomMessage).IsAssignableFrom(t));
            conventions.DefiningDataBusPropertiesAs(t =>
                t.GetCustomAttributes(typeof(CustomDataBusAttribute), false).Length > 0);
            
            var rabbitMq = Environment.GetEnvironmentVariable("USE_RMQ");
            TransportExtensions trasport = null;

            if (string.IsNullOrEmpty(rabbitMq))
            {
                trasport = endpointConfig.UseTransport<LearningTransport>();
            }
            else
            {
                var rabbitMqHostname = Environment.GetEnvironmentVariable("RMQ_HOST") ?? "rabbitmq";
                trasport = endpointConfig.UseTransport<RabbitMQTransport>()
                    .ConnectionString($"host={rabbitMqHostname}")
                    .UseConventionalRoutingTopology();
            }

            var persistence = endpointConfig.UsePersistence<MongoPersistence>().UseTransactions(false);
                    
            persistence.MongoClient(new MongoClient(Environment.GetEnvironmentVariable($"{endpointName}MongoDb") ?? "mongodb://localhost:27017"));
                    
            persistence.DatabaseName($"{endpointName}Persistence");
                    
            /* routing messages */
            var routing = trasport.Routing();

            endpointConfig.EnableInstallers();
            return routing;
        }
    }
}