using System;
using MongoDB.Driver;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;

namespace Shared
{
    public static class NServiceBusConfiguration
    {
        public static RoutingSettings Configure(this EndpointConfiguration endpointConfig, string endpointName)
        {
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