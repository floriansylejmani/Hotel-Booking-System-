using HotelBooking.Application.Common.Interfaces;
using HotelBooking.Persistence.Database;
using HotelBooking.Persistence.Context;
using HotelBooking.Persistence.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HotelBooking.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<DatabaseSettings>()
            .Configure(options =>
            {
                options.ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            })
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "Connection string 'DefaultConnection' was not found.")
            .ValidateOnStart();

        var databaseSettings = new DatabaseSettings
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty
        };

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                databaseSettings.ConnectionString,
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<AppDbInitializer>();
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>(
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "ready"]);

        return services;
    }
}
