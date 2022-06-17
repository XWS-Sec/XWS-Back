using System;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;

namespace Shared.CustomCounters
{
    public static class MetricRegistry
    {
        public static CounterOptions SuccessfulStatusCounter => new CounterOptions()
        {
            Name = "successful_status_counter",
            Context = "BaseApi",
            MeasurementUnit = Unit.Events,
        };

        public static CounterOptions BadRequestStatusCounter => new CounterOptions()
        {
            Name = "bad_request_status_counter",
            Context = "BaseApi",
            MeasurementUnit = Unit.Events,
        };

        public static CounterOptions NotFoundStatusCounter => new CounterOptions()
        {
            Name = "not_found_status_counter",
            Context = "BaseApi",
            MeasurementUnit = Unit.Events
        };
        
        public static CounterOptions InternalServerErrors => new CounterOptions()
        {
            Name = "internal_server_errors_counter",
            Context = "BaseApi",
            MeasurementUnit = Unit.Events
        };

        public static CounterOptions ThroughPutRequestCounter => new CounterOptions()
        {
            Name = "throughput_request",
            Context = "BaseApi",
            MeasurementUnit = Unit.Bytes
        };

        public static CounterOptions ThroughPutResponseCounter => new CounterOptions()
        {
            Name = "throughput_response",
            Context = "BaseApi",
            MeasurementUnit = Unit.Bytes
        };

        public static CounterOptions DistinctIpCounterOptions => new CounterOptions()
        {
            Context = "BaseApi",
            Name = "distinct_ip_counter",
            MeasurementUnit = Unit.Calls
        };

        public static CounterOptions DistinctBrowserCounterOptions => new CounterOptions()
        {
            Context = "BaseApi",
            Name = "distinct_browser_counter",
            MeasurementUnit = Unit.Calls
        };

        public static CounterOptions DistinctUsers => new CounterOptions()
        {
            Context = "BaseApi",
            Name = "distinct_user_counter",
            MeasurementUnit = Unit.Calls
        };

        public static MetricTags BundleTags(string ip, string browser, DateTime timestamp) =>
            new MetricTags(new[] { "ip", "browser" }, new[] { ip, browser });
        
        public static MetricTags BundleTags(string name, string value) =>
            new MetricTags(new[] { name }, new[] { value });
    }
}