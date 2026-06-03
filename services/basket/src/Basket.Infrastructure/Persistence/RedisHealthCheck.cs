using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Basket.Infrastructure.Persistence;

internal sealed class RedisHealthCheck(IDistributedCache cache) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"health:{Guid.NewGuid()}";
            await cache.SetStringAsync(key, "ok", cancellationToken);
            await cache.RemoveAsync(key, cancellationToken);

            return HealthCheckResult.Healthy("Redis is reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Redis is not reachable.", exception);
        }
    }
}
