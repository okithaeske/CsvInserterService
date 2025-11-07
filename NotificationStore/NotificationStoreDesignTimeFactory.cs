using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationStore;

public class NotificationStoreDesignTimeFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("NOTIFICATION_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=notificationdb;Username=postgres;Password=okitha123";

        optionsBuilder.UseNpgsql(connectionString);
        return new NotificationDbContext(optionsBuilder.Options);
    }
}
