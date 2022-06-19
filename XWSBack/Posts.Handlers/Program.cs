using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NServiceBus;
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
                                options.InfluxDb.Database = "metricsdatabase-posts";
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
                    var certPath = Environment.GetEnvironmentVariable("XWS_PKI_ROOT_CERT_FOLDER") ??
                                   @"%USERPROFILE%\.xws-cert\";
                    var pfxPath = Environment.ExpandEnvironmentVariables(certPath) + "apiCert.pfx";
                    var certPass = Environment.GetEnvironmentVariable("XWS_PKI_ADMINPASS");

                    var certificate = new X509Certificate2(
                        pfxPath,
                        certPass);


                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Loopback, 44326,
                            listenOptions => { listenOptions.UseHttps(certificate); });
                        options.ListenLocalhost(4003);
                    });
                }).UseNServiceBus(ctx =>
                {
                    var endpointConfig = new EndpointConfiguration(EndpointInstances.PostHandlers);
                    endpointConfig.Configure(EndpointInstances.PostHandlers);

                    return endpointConfig;
                });
        }
    }
}