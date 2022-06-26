using System.IO;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.AspNetCore.Http;
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
            await _next.Invoke(context);

            using var memoryStream = new MemoryStream();
            await memoryStream.CopyToAsync(context.Response.Body);

            using var memoryStreamBody = new MemoryStream();
            await memoryStreamBody.CopyToAsync(context.Request.Body);

            _metrics.Measure.Counter.Increment(MetricRegistry.ThroughPutRequestCounter, memoryStreamBody.Length + context.Request.Headers.ToString()?.Length ?? 0);
            _metrics.Measure.Counter.Increment(MetricRegistry.ThroughPutResponseCounter, memoryStream.Length + context.Response.Headers.ToString()?.Length ?? 0);
        }
    }
}