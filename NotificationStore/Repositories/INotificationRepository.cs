using NotificationStore.Entities;

namespace NotificationStore.Repositories;

public interface INotificationRepository
{
    Task<NotificationRecord> AddAsync(NotificationRecord record, CancellationToken cancellationToken = default);
    Task MarkDeliveredAsync(Guid id, DateTimeOffset deliveredAt, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, DateTimeOffset readAt, CancellationToken cancellationToken = default);
}
