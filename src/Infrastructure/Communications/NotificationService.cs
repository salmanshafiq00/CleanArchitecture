using Application.Common.Abstractions;
using Application.Common.Abstractions.Communication;
using Application.Features.Admin.AppNotifications.Queries;
using Domain.Admin;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Communications;

internal sealed class NotificationService(
    IHubContext<NotificationHub, INotificationHub> hubContext,
    IApplicationDbContext dbContext,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task SendNotificationAsync(AppNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var notifyModel = notification.Adapt<AppNotificationModel>();

            if (notification.RecieverId != null)
            {
                // Send to specific user
                await hubContext.Clients.Group(notification.RecieverId)
                    .ReceiveNotification(notifyModel);
            }
            else if (notification.Group != null)
            {
                // Send to specific group
                await hubContext.Clients.Group(notification.Group)
                    .ReceiveNotification(notifyModel);
            }
            else
            {
                // Send to all users
                await hubContext.Clients.All
                    .ReceiveNotification(notifyModel);
            }

            notification.IsProcessed = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending notification {NotificationId}", notification.Id);
            notification.RetryCount++;
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
    }

    public async Task ProcessPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var pendingNotifications = await dbContext.AppNotifications
            .Where(n => !n.IsProcessed && n.RetryCount < 3)
            .ToListAsync(cancellationToken);

        foreach (var notification in pendingNotifications)
        {
            try
            {
                await SendNotificationAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                {
                    logger.LogError(ex, "Error sending notification {NotificationId}", notification.Id);
                    notification.RetryCount++;
                    await dbContext.SaveChangesAsync(cancellationToken);
                    throw;
                }
            }
        }
    }
}
