using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApiModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Shared;

namespace ChatsApiEndpoint
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatsApiEndpoint", Version = "v1" });
            });

            services.AddSingleton<IMongoClient, MongoClient>(s => SetupMongoDb.CreateClient<Chat>("ChatApi", "Chats"));
            var rabbitMq = Environment.GetEnvironmentVariable("USE_RMQ");
            if (!string.IsNullOrEmpty(rabbitMq))
            {
                var rabbitMqHostname = Environment.GetEnvironmentVariable("RMQ_HOST") ?? "rabbitmq";
                services.AddSingleton<IHostedService>(new ProceedIfRabbitMqIsAlive(rabbitMqHostname));
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatsApiEndpoint v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}