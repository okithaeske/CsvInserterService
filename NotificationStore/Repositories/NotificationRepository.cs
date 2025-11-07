using Microsoft.EntityFrameworkCore;
using NotificationStore.Entities;

namespace NotificationStore.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationRecord> AddAsync(NotificationRecord record, CancellationToken cancellationToken = default)
    {
        if (record.Id == Guid.Empty)
        {
            record.Id = Guid.NewGuid();
        }

        await _context.Notifications.AddAsync(record, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task MarkDeliveredAsync(Guid id, DateTimeOffset deliveredAt, CancellationToken cancellationToken = default)
    {
        await _context.Notifications
            .Where(n => n.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.Status, _ => NotificationStatus.Delivered)
                .SetProperty(n => n.DeliveredAt, _ => deliveredAt), cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid id, DateTimeOffset readAt, CancellationToken cancellationToken = default)
    {
        await _context.Notifications
            .Where(n => n.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, _ => true)
                .SetProperty(n => n.ReadAt, _ => readAt), cancellationToken);
    }
}
