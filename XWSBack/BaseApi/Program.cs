using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Chats.Messages;
using JobOffers.Messages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Neo4jClient.Extensions;
using NServiceBus;
using Posts.Messages;
using Serilog;
using Serilog.Events;
using Shared;
using Users.Graph.Messages;
using Users.Graph.Messages.Follow;
using Users.Graph.Messages.Skills;

namespace BaseApi
{
    public class Program
    {
        public static int Main(string[] args)
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

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureMetricsWithDefaults(builder =>
                {
                    var influxdb = Environment.GetEnvironmentVariable("InfluxDBUri") ?? "http://localhost:8086";
                    var client = new HttpClient()
                    {
                        BaseAddress = new Uri(influxdb)
                    };

                    try
                    {
                        var response = client.GetAsync("/ping").GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode)
                        {
                            builder.Report.ToInfluxDb(options =>
                            {
                                options.FlushInterval = TimeSpan.FromSeconds(10);
                                options.InfluxDb.Database = "metricsdatabase-baseapi";
                                options.InfluxDb.BaseUri = new Uri(influxdb);
                                options.InfluxDb.UserName = Environment.GetEnvironmentVariable("InfluxUser") ?? "admin";
                                options.InfluxDb.Password =
                                    Environment.GetEnvironmentVariable("InfluxPass") ?? "admin123";
                                options.InfluxDb.CreateDataBaseIfNotExists = true;
                            });
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        Log.Logger.Warning("Could not find InfluxDB to store metrics. Aborted configuring the report.");
                    }
                })
                .UseMetricsWebTracking()
                .UseMetrics(options =>
                {
                    options.EndpointOptions = endpointOpts =>
                    {
                        endpointOpts.MetricsTextEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
                        endpointOpts.MetricsEndpointOutputFormatter = new MetricsPrometheusProtobufOutputFormatter();
                    };
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var useHttps = Environment.GetEnvironmentVariable("USE_HTTPS") ?? "true";
                    var certPath = Environment.GetEnvironmentVariable("XWS_PKI_ROOT_CERT_FOLDER") ??
                                   @"%USERPROFILE%\.xws-cert\";
                    var pfxPath = Environment.ExpandEnvironmentVariables(certPath) + "apiCert.pfx";
                    var certPass = Environment.GetEnvironmentVariable("XWS_PKI_ADMINPASS");

                    X509Certificate2 certificate = null;
                    if (useHttps == "true")
                    {
                        certificate = new X509Certificate2(
                            pfxPath,
                            certPass);
                    }
                    
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(options =>
                    {
                        if (useHttps == "true")
                        {
                            options.Listen(IPAddress.Loopback, 44322,
                                listenOptions => { listenOptions.UseHttps(certificate); });   
                        }
                        options.ListenAnyIP(7700);
                    });
                })
                .UseNServiceBus(context =>
                {
                    var endpointConfig = new EndpointConfiguration(EndpointInstances.BaseApiEndpoint);
                    var routing = endpointConfig.Configure(EndpointInstances.BaseApiEndpoint);

                    routing.RouteToEndpoint(typeof(CreateNodeRequest), EndpointInstances.UserGraphHandlers);
                    routing.RouteToEndpoint(typeof(CreateFollowLinkRequest), EndpointInstances.UserGraphHandlers);
                    routing.RouteToEndpoint(typeof(AnswerFollowRequest), EndpointInstances.UserGraphHandlers);
                    routing.RouteToEndpoint(typeof(GetFollowStatsRequest), EndpointInstances.UserGraphHandlers);
                    routing.RouteToEndpoint(typeof(UnfollowRequest), EndpointInstances.UserGraphHandlers);
                    routing.RouteToEndpoint(typeof(AdjustSkillsRequest), EndpointInstances.UserGraphHandlers);
                    routing.RouteToEndpoint(typeof(GetSkillsRequest), EndpointInstances.UserGraphHandlers);
                    routing.RouteToEndpoint(typeof(BlockRequest), EndpointInstances.UserGraphHandlers);
                    routing.RouteToEndpoint(typeof(UnblockRequest), EndpointInstances.UserGraphHandlers);
                    routing.RouteToEndpoint(typeof(RecommendNewLinksRequest), EndpointInstances.UserGraphHandlers);
                    
                    routing.RouteToEndpoint(typeof(NewPostRequest), EndpointInstances.PostHandlers);
                    routing.RouteToEndpoint(typeof(EditPostRequest), EndpointInstances.PostHandlers);
                    routing.RouteToEndpoint(typeof(GetPostsRequest), EndpointInstances.PostHandlers);
                    routing.RouteToEndpoint(typeof(CommentRequest), EndpointInstances.PostHandlers);
                    routing.RouteToEndpoint(typeof(GetUserByPostRequest), EndpointInstances.PostHandlers);
                    routing.RouteToEndpoint(typeof(LikeDislikeRequest), EndpointInstances.PostHandlers);
                    
                    routing.RouteToEndpoint(typeof(AddMessageRequest), EndpointInstances.ChatHandlers);
                    routing.RouteToEndpoint(typeof(GetChatRequest), EndpointInstances.ChatHandlers);
                    
                    routing.RouteToEndpoint(typeof(CreateCompanyRequest), EndpointInstances.JobOffersHandlers);
                    routing.RouteToEndpoint(typeof(PublishNewJobOfferRequest), EndpointInstances.JobOffersHandlers);
                    routing.RouteToEndpoint(typeof(GetBasicJobOffersRequest), EndpointInstances.JobOffersHandlers);
                    routing.RouteToEndpoint(typeof(GetRecommendedJobOffersRequest), EndpointInstances.JobOffersHandlers);

                    return endpointConfig;
                });
        }
    }
}