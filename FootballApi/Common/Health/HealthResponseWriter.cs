using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FootballApi.Common.Health;

public static class HealthResponseWriter
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.Headers.CacheControl = "no-store";

        var body = new
        {
            status = report.Status == HealthStatus.Healthy ? "ok" : "unavailable",
            checks = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString().ToLowerInvariant(),
                    description = e.Value.Description,
                    durationMs = (long)e.Value.Duration.TotalMilliseconds
                })
        };

        return context.Response.WriteAsJsonAsync(body);
    }
}