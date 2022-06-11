using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ganss.XSS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BaseApi.Middleware
{
    public class AntiXssMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AntiXssMiddleware> _logger;
        private const string XssTemplate = "XSS attempt detected at {0} in {1}";
        private const string Response = "XSS injection detected from middleware in {0}, automatically rejecting";
        private const string Query = "QueryParams";
        private const string Body = "RequestBody";

        public AntiXssMiddleware(RequestDelegate next, ILogger<AntiXssMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value.EndsWith("Picture") || 
                httpContext.Request.Path.Value.Contains("Post"))
            {
                await _next.Invoke(httpContext);
                return;
            }
        
            httpContext.Request.EnableBuffering();
            var sanitizer = new HtmlSanitizer();

            using (var streamReader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var raw = await streamReader.ReadToEndAsync();
                raw = raw.Replace("\r\n", "\n");

                var sanitized = sanitizer.Sanitize(raw);
                if (raw != sanitized)
                {
                    await SendBadRequest(httpContext, Body);
                    return;
                }
            }

            foreach (var queryParam in httpContext.Request.Query.Select(x => x.Value))
            {
                var raw = queryParam;
                var sanitized = sanitizer.Sanitize(raw);
                if (raw != sanitized)
                {
                    await SendBadRequest(httpContext, Query);
                    return;
                }
            }

            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            await _next.Invoke(httpContext);
        }

        private async Task SendBadRequest(HttpContext httpContext, string where)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadGateway;
            await httpContext.Response.WriteAsJsonAsync(string.Format(Response, where));
            _logger.LogInformation(string.Format(XssTemplate, DateTime.Now, where));
        }
    }
}