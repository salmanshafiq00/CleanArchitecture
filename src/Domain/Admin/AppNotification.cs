using Domain.Abstractions;

namespace Domain.Admin;

public sealed class AppNotification : BaseAuditableEntity
{
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string SenderId { get; set; }
    public string? RecieverId { get; set; }
    public string? Group { get; set; }
    public bool IsSeen { get; set; }
    public bool IsProcessed { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
    //public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
}
