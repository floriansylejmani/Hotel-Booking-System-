using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Persistence.Database;

public static class ServiceProviderExtensions
{
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<AppDbInitializer>();
        await initializer.InitializeAsync(cancellationToken);
    }
}
