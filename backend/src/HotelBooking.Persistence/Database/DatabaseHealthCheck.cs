using HotelBooking.Persistence.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HotelBooking.Persistence.Database;

public sealed class DatabaseHealthCheck(AppDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? HealthCheckResult.Healthy("PostgreSQL connection is available.")
            : HealthCheckResult.Unhealthy("PostgreSQL connection is unavailable.");
    }
}
