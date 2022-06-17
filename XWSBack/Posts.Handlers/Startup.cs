using System;
using System.Linq;
using App.Metrics;
using App.Metrics.Formatters.InfluxDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Posts.Model;
using Shared;

namespace Posts.Handlers
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton<IMongoClient, MongoClient>(s => SetupMongoDb.CreateClient<Post>("PostsHandlers", "Posts"));
            BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));
                    
            var serviceInstances = typeof(Program).Assembly.GetTypes()
                .Where(x => x.Namespace.EndsWith("Services") && x.Name.EndsWith("Service"));

            foreach (var serviceInstance in serviceInstances)
            {
                services.AddSingleton(serviceInstance);
            }

            services.AddAutoMapper(typeof(Program));
            
            var influxUri = Environment.GetEnvironmentVariable("InfluxDB") ?? "http://localhost:8086";
            
            services.AddAppMetricsSystemMetricsCollector();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}