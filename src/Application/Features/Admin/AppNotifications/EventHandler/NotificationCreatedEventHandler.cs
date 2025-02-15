using Application.Common.Abstractions;
using Domain.Admin;

namespace Application.Features.Admin.AppNotifications.EventHandler;

public sealed class NotificationCreatedEventHandler(
    IApplicationDbContext dbContext)
    : INotificationHandler<NotificationCreatedEvent>
{

    public async Task Handle(NotificationCreatedEvent notification, CancellationToken cancellationToken)
    {
        dbContext.AppNotifications.Add(notification.Notification);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
