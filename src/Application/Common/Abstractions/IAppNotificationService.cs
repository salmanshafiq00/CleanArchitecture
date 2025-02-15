using Domain.Admin;

namespace Application.Common.Abstractions;

internal interface IAppNotificationService
{
    int Create(AppNotification notification);
    int Update(AppNotification notification);
    int UpdateAllUnseen(AppNotification notification);
}
