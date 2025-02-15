using Domain.Admin;

namespace Application.Common.Abstractions.Communication;

public interface INotificationService
{
    Task SendNotificationAsync(AppNotification notification, CancellationToken cancellationToken = default);
    Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default);
}
