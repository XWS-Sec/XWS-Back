using System.Threading.Tasks;
using App.Metrics;
using Microsoft.AspNetCore.Http;
using Neo4jClient.Extensions;
using Shared.CustomCounters;

namespace BaseApi.Middleware
{
    public class CustomStatusCodeCounterMiddleware
    {
        private readonly IMetrics _metrics;
        private readonly RequestDelegate _next;
        
        
        public CustomStatusCodeCounterMiddleware(IMetrics metrics, RequestDelegate next)
        {
            _metrics = metrics;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            await _next.Invoke(context);
            
            var response = context.Response;

            if (response.StatusCode >= 200 && response.StatusCode < 400)
            {
                _metrics.Measure.Counter.Increment(MetricRegistry.SuccessfulStatusCounter, 1);
            }
            else if (response.StatusCode >= 400 && response.StatusCode <= 500)
            {
                _metrics.Measure.Counter.Increment(MetricRegistry.BadRequestStatusCounter, 1);
                if (response.StatusCode == 500)
                {
                    _metrics.Measure.Counter.Increment(MetricRegistry.InternalServerErrors, 1);
                }

                if (response.StatusCode == 404)
                {
                    _metrics.Measure.Counter.Increment(MetricRegistry.NotFoundStatusCounter, 1);
                }
            }
        }
    }
}