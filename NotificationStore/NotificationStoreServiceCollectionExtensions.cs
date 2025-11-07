using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NotificationStore.Repositories;

namespace NotificationStore;

public static class NotificationStoreServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationStore(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Notification store connection string is missing or empty.");
        }

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<INotificationRepository, NotificationRepository>();

        return services;
    }
}
