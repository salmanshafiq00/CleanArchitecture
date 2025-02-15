namespace Application.Features.Admin.AppNotifications.Queries;

public record AppNotificationModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string SenderId { get; set; }
    public string RecieverId { get; set; }
    public bool IsSeen { get; set; }
    public bool IsProcessed { get; set; }
    public string? Group { get; set; }
    public int RetryCount { get; set; }
    public DateTime Created { get; set; }
    public string? CreatedBy { get; set; }
    public string TimeAgo => GetTimeAgo(Created);

    private static string GetTimeAgo(DateTime created)
    {
        var timeSpan = DateTime.Now - created;

        if (timeSpan.TotalSeconds < 60)
            return $"{(int)timeSpan.TotalSeconds} seconds ago";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} minutes ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} hours ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} days ago";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} months ago";

        return $"{(int)(timeSpan.TotalDays / 365)} years ago";
    }
}
