using Application.Common.Security;
using Application.Features.Admin.AppNotifications.Queries;
using Microsoft.AspNetCore.SignalR;
using Application.Common.Abstractions.Communication;
using System.Security.Claims;

namespace Infrastructure.Communications;

[Authorize]
public class NotificationHub : Hub<INotificationHub>
{
    public async Task SendNotification(AppNotificationModel notification)
    {
        // Broadcast to all connected clients
        if (string.IsNullOrEmpty(notification.RecieverId) && string.IsNullOrEmpty(notification.Group))
        {
            await Clients.All.ReceiveNotification(notification);
        }
        // Send to a specific group
        else if (!string.IsNullOrEmpty(notification.Group))
        {
            await Clients.Group(notification.Group).ReceiveNotification(notification);
        }
        // Send to a specific user
        else if (!string.IsNullOrEmpty(notification.RecieverId))
        {
            await Clients.User(notification.RecieverId).ReceiveNotification(notification);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // Add connection to a group named after the user's ID
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            // Optional: Notify the user of a successful connection
            await Clients.User(userId).ReceiveNotification(new AppNotificationModel
            {
                Title = "Welcome",
                Description = "You are now connected to the notification hub.",
                RecieverId = userId,
                Created = DateTime.UtcNow
            });
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            // Remove the connection from the user's group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}

