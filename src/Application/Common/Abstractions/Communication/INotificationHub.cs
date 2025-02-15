using Application.Features.Admin.AppNotifications.Queries;

namespace Application.Common.Abstractions.Communication;

public interface INotificationHub
{
    Task ReceiveNotification(AppNotificationModel notification);
    Task ReceiveRolePermissionNotify();
    Task ReceiveRoleMenuNotify();
    Task ReceiveMenuOrderChangeNotify();
}
