namespace Application.Common.Events;

internal sealed class CacheInvalidationEvent : INotification
{
    public string[] CacheKeys { get; set; } = [];
}
