using Microsoft.EntityFrameworkCore;
using NotificationStore.Entities;

namespace NotificationStore;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NotificationRecord>(builder =>
        {
            builder.ToTable("notifications");
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Type)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(n => n.Title)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(n => n.Body)
                .HasMaxLength(2048)
                .IsRequired();

            builder.Property(n => n.Status)
                .HasConversion(
                    value => value.ToString(),
                    value => Enum.Parse<NotificationStatus>(value))
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(n => n.RecipientId)
                .HasMaxLength(128);

            builder.Property(n => n.IsRead)
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            builder.Property(n => n.DeliveredAt);
            builder.Property(n => n.ReadAt);
        });
    }
}
