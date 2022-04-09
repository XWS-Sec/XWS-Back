using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using Shared;

namespace BaseApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var certPath = Environment.GetEnvironmentVariable("XWS_PKI_ROOT_CERT_FOLDER") ?? @"%USERPROFILE%\.xws-cert\";
                    var pfxPath = Environment.ExpandEnvironmentVariables(certPath) + "apiCert.pfx";
                    var certPass = Environment.GetEnvironmentVariable("XWS_PKI_ADMINPASS");

                    var certificate = new X509Certificate2(
                        pfxPath,
                        certPass);

                    
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Loopback, 44322, listenOptions =>
                        {
                            listenOptions.UseHttps(certificate);
                        });
                    });
                })
                .UseNServiceBus(context =>
                {
                    var endpointConfig = new EndpointConfiguration(EndpointInstances.BaseApiEndpoint);
                    var routing = endpointConfig.Configure(EndpointInstances.BaseApiEndpoint);

                    return endpointConfig;
                });
    }
}