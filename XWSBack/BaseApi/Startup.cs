using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BaseApi.CustomAttributes;
using BaseApi.Hubs;
using BaseApi.Middleware;
using BaseApi.Model.Mongo;
using BaseApi.Services.ConfigurationContracts;
using BaseApi.Services.Extensions;
using BaseApi.Services.PasswordlessSettings;
using BaseApi.Services.TokenProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Neo4jClient;
using Serilog;
using Shared;
using SignalRSwaggerGen.Enums;

namespace BaseApi
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
            var localhost = Environment.GetEnvironmentVariable("LCH") ?? "localhost";
            ProceedIfServiceIsAlive checker = null;
            var useRmq = Environment.GetEnvironmentVariable("USE_RMQ");
            if (!string.IsNullOrEmpty(useRmq))
            {
                Log.Logger.Information("Trying to connect to rabbitmq");
                checker = new ProceedIfServiceIsAlive(localhost, 5672);
                checker.Check();
                Log.Logger.Information("Connected to rabbitmq");
            }
        
            Log.Logger.Information("Trying to connect to mongodb");
            checker = new ProceedIfServiceIsAlive(localhost, 27017);
            checker.Check();
            Log.Logger.Information("Connected to mongodb");
            
            services.AddSignalR();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "BaseApi", Version = "v1" });
            });

            var mongoConnectionString =
                Environment.GetEnvironmentVariable("BaseApiMongoDb") ?? "mongodb://localhost:27017";

            services.AddSingleton<IMongoClient, MongoClient>(s => SetupMongoDb.CreateClient<User>("BaseApi", "Users"));

            services.AddIdentity<User, Role>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.User.RequireUniqueEmail = true;
                }).AddDefaultTokenProviders()
                .AddTokenProvider<NPTokenProvider<User>>("NPTokenProvider")
                .AddMongoDbStores<User, Role, Guid>(mongoConnectionString, "Users");

            services.AddAutoMapper(typeof(Startup));

            var allServices = typeof(ImageExtensions).Assembly
                .GetTypes()
                .Where(x => x.Name.EndsWith("Service"));

            foreach (var service in allServices) services.AddScoped(service);

            var picUserDir = Environment.GetEnvironmentVariable("USER_PIC_DIR") ?? @"%USERPROFILE%\.xws-user-pics";
            var picPostDir = Environment.GetEnvironmentVariable("POST_PIC_DIR") ?? @"%USERPROFILE%\.xws-post-pics";

            var expanded = Environment.ExpandEnvironmentVariables(picPostDir);
            if (!Directory.Exists(expanded)) Directory.CreateDirectory(expanded);

            expanded = Environment.ExpandEnvironmentVariables(picUserDir);
            if (!Directory.Exists(expanded)) Directory.CreateDirectory(expanded);
            
            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(_ => true);
            }));

            services.AddSingleton(new SendGridContract());

            services.AddSingleton(new FacebookAuthSettings());
            services.AddHttpClient();

            services.AddMemoryCache();

            services.AddSingleton<MemoryCacheEntryOptions>(x => new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(1),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromHours(1),
            });

            services.AddMetrics();
            services.AddMetricsEndpoints();
            services.AddMetricsTrackingMiddleware();
            services.AddMetricsReportingHostedService();
            services.AddAppMetricsCollectors();
            services.AddAppMetricsHealthPublishing();
            services.AddAppMetricsSystemMetricsCollector();
            services.AddAppMetricsGcEventsMetricsCollector();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BaseApi v1"));
            
            var useHttps = Environment.GetEnvironmentVariable("USE_HTTPS") ?? "true";
            if (useHttps == "true")
            {
                app.UseHttpsRedirection();   
            }

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();
            
            app.UseCors("CorsPolicy");

            app.UseMiddleware<AntiXssMiddleware>();
            app.UseMiddleware<CustomStatusCodeCounterMiddleware>();
            app.UseMiddleware<CustomThroughPutCalculationMiddleware>();
            app.UseMiddleware<CustomDistinctUserCounterMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<BaseHub>("/hub");
                endpoints.MapControllers();
            });
        }
    }
}