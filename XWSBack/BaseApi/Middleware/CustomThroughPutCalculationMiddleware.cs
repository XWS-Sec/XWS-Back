using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.AspNetCore.Http;
using SendGrid;
using Shared.CustomCounters;

namespace BaseApi.Middleware
{
    public class CustomThroughPutCalculationMiddleware
    {
        
        private readonly IMetrics _metrics;
        private readonly RequestDelegate _next;

        public CustomThroughPutCalculationMiddleware(IMetrics metrics, RequestDelegate next)
        {
            _metrics = metrics;
            _next = next;
        }
        
        public async Task Invoke(HttpContext context)
        {
            var originalStream = context.Response.Body;

            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;
            
            await _next.Invoke(context);

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalStream);
            context.Response.Body = originalStream;
            
            _metrics.Measure.Counter.Increment(MetricRegistry.ThroughPutRequestCounter, context.Request.ContentLength + context.Request.Headers.ToString()?.Length ?? 0);
            _metrics.Measure.Counter.Increment(MetricRegistry.ThroughPutResponseCounter, memoryStream.Length + context.Response.Headers.ToString()?.Length ?? 0);
        }
    }
}