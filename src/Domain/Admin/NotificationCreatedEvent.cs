using Domain.Abstractions;

namespace Domain.Admin;

public class NotificationCreatedEvent(
    AppNotification notification) : BaseEvent
{
    public AppNotification Notification { get; } = notification;
}
