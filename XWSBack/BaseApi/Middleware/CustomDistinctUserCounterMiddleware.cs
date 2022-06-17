using System;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.AspNetCore.Http;
using Shared.CustomCounters;

namespace BaseApi.Middleware
{
    public class CustomDistinctUserCounterMiddleware
    {
        private readonly IMetrics _metrics;
        private readonly RequestDelegate _next;

        public CustomDistinctUserCounterMiddleware(IMetrics metrics, RequestDelegate next)
        {
            _metrics = metrics;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next.Invoke(context);

            var ip = context.Connection.RemoteIpAddress;
            var browser = context.Request.Headers["user-agent"].FirstOrDefault() ?? "Unknown";
            
            _metrics.Measure.Counter.Increment(MetricRegistry.DistinctIpCounterOptions, MetricRegistry.BundleTags("ip", ip?.ToString()));
            _metrics.Measure.Counter.Increment(MetricRegistry.DistinctBrowserCounterOptions, MetricRegistry.BundleTags("browser", browser));
            _metrics.Measure.Counter.Increment(MetricRegistry.DistinctUsers, MetricRegistry.BundleTags(ip?.ToString(), browser, DateTime.Now));
        }
    }
}