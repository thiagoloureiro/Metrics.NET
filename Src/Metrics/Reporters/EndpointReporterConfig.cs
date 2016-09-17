﻿using System;
using System.Net;
using System.Text;
using Metrics.Json;
using Metrics.MetricData;
using Metrics.Reports;
using Metrics.Visualization;

namespace Metrics.Reporters
{
    internal static class EndpointReporterConfig
    {
        public static MetricsEndpointReports WithTextReport(this MetricsEndpointReports reports, string endpoint)
        {
            return reports.WithEndpointReport(endpoint, (d, h, c) => new MetricsEndpointResponse(StringReport.RenderMetrics(d, h), "text/plain"));
        }

        public static MetricsEndpointReports WithJsonHealthReport(this MetricsEndpointReports reports, string endpoint)
        {
            return reports.WithEndpointReport(endpoint, GetHealthResponse);
        }

        private static MetricsEndpointResponse GetHealthResponse(MetricsData data, Func<HealthStatus> healthStatus, MetricsEndpointRequest request)
        {
            var status = healthStatus();
            var json = JsonHealthChecks.BuildJson(status);

            var httpStatus = status.IsHealthy ? 200 : 500;
            var httpStatusDescription = status.IsHealthy ? "OK" : "Internal Server Error";

            return new MetricsEndpointResponse(json, JsonHealthChecks.HealthChecksMimeType, Encoding.UTF8, httpStatus, httpStatusDescription);
        }

        public static MetricsEndpointReports WithJsonV1Report(this MetricsEndpointReports reports, string endpoint)
        {
            return reports.WithEndpointReport(endpoint, GetJsonV1Response);
        }

        private static MetricsEndpointResponse GetJsonV1Response(MetricsData data, Func<HealthStatus> healthStatus, MetricsEndpointRequest request)
        {
            var json = JsonBuilderV1.BuildJson(data);
            return new MetricsEndpointResponse(json, JsonBuilderV1.MetricsMimeType);
        }

        public static MetricsEndpointReports WithJsonV2Report(this MetricsEndpointReports reports, string endpoint)
        {
            return reports.WithEndpointReport(endpoint, GetJsonV2Response);
        }

        private static MetricsEndpointResponse GetJsonV2Response(MetricsData data, Func<HealthStatus> healthStatus, MetricsEndpointRequest request)
        {
            var json = JsonBuilderV2.BuildJson(data);
            return new MetricsEndpointResponse(json, JsonBuilderV2.MetricsMimeType);
        }

        public static MetricsEndpointReports WithJsonReport(this MetricsEndpointReports reports, string endpoint)
        {
            return reports.WithEndpointReport(endpoint, GetJsonResponse);
        }

        private static MetricsEndpointResponse GetJsonResponse(MetricsData data, Func<HealthStatus> healthStatus, MetricsEndpointRequest request)
        {
            string acceptHeader;
            if (request.Headers.TryGetValue("Accept", out acceptHeader))
            {
                return acceptHeader.Contains(JsonBuilderV2.MetricsMimeType)
                    ? GetJsonV2Response(data, healthStatus, request)
                    : GetJsonV1Response(data, healthStatus, request);
            }

            return GetJsonV1Response(data, healthStatus, request);
        }
    }
}