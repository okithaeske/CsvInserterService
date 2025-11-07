namespace NotificationStore.Entities;

public class NotificationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? RecipientId { get; set; }
    public string Type { get; set; } = "system";
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeliveredAt { get; set; }
}
