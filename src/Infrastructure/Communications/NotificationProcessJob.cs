using Application.Common.Abstractions.Communication;
using Application.Common.Abstractions;
using Microsoft.AspNetCore.SignalR;
using Domain.Admin;
using System.Data;
using Dapper;
using Application.Features.Admin.AppNotifications.Queries;
using Infrastructure.Persistence;

namespace Infrastructure.Communications;

public sealed class NotificationProcessJob(
    ISqlConnectionFactory sqlConnectionFactory,
    IHubContext<NotificationHub, INotificationHub> hubContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<NotificationProcessJob> logger)
{
    private const int BatchSize = 15;
    private const int MaxRetries = 3;

    public async Task ProcessNotificationsAsync()
    {
        try
        {
            logger.LogInformation("Beginning notification processing at {Time}",
            dateTimeProvider.Now);

            using var connection = sqlConnectionFactory.GetOpenConnection();
            using var transaction = connection.BeginTransaction();

            var notifications = await GetPendingNotificationsAsync(connection, transaction);

            if (!notifications.Any())
            {
                logger.LogInformation("No pending notifications to process");
                return;
            }

            foreach (var notification in notifications)
            {
                try
                {
                    var success = await SendNotificationViaSignalR(notification);

                    if (success)
                    {
                        await MarkNotificationAsProcessed(connection, transaction, notification);
                        logger.LogInformation("Notification {Id} processed successfully", notification.Id);
                    }
                    else
                    {
                        await HandleFailedNotification(connection, transaction, notification, "Failed to send via SignalR");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing notification {Id}", notification.Id);
                    await HandleFailedNotification(connection, transaction, notification, ex.Message);
                }
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in notification processing job");
            throw;
        }
    }


    private async Task<IReadOnlyList<AppNotification>> GetPendingNotificationsAsync(
            IDbConnection connection,
            IDbTransaction transaction)
    {
        const string sql = """
            SELECT TOP (@BatchSize) 
                Id, Title, Description, Url, SenderId, RecieverId, [Group],
                IsSeen, IsProcessed, RetryCount, Error
            FROM dbo.AppNotifications WITH (READPAST)
            WHERE IsProcessed = 0
                AND RetryCount < @MaxRetries
                AND (LastModified IS NULL OR DATEADD(SECOND, POWER(2, RetryCount) * 10, LastModified) < @CurrentTime)
            ORDER BY Created, RetryCount
        """;

        return (await connection.QueryAsync<AppNotification>(
            sql,
            new
            {
                BatchSize,
                MaxRetries,
                CurrentTime = dateTimeProvider.Now
            },
            transaction: transaction)).ToList();
    }

    private async Task<bool> SendNotificationViaSignalR(AppNotification notification)
    {
        try
        {
            var model = new AppNotificationModel
            {
                Id = notification.Id,
                Title = notification.Title,
                Description = notification.Description,
                SenderId = notification.SenderId,
                RecieverId = notification.RecieverId,
                IsSeen = notification.IsSeen,
                Url = notification.Url
            };

            if (!string.IsNullOrEmpty(notification.Group))
            {
                await hubContext.Clients.Group(notification.Group).ReceiveNotification(model);
            }
            else if (string.IsNullOrEmpty(notification.RecieverId))
            {
                await hubContext.Clients.All.ReceiveNotification(model);
            }
            else
            {
                await hubContext.Clients.User(notification.RecieverId).ReceiveNotification(model);
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send notification via SignalR {Id}", notification.Id);
            return false;
        }
    }

    private async Task MarkNotificationAsProcessed(
        IDbConnection connection,
        IDbTransaction transaction,
        AppNotification notification)
    {
        const string sql = """
        UPDATE dbo.AppNotifications
        SET 
            IsProcessed = 1,
            LastModified = @LastModified,
            LastModifiedBy = 'NotificationProcessor',
            Error = NULL
        WHERE Id = @Id
        """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                notification.Id,
                LastModified = dateTimeProvider.Now
            },
            transaction: transaction);
    }

    private async Task HandleFailedNotification(
        IDbConnection connection,
        IDbTransaction transaction,
        AppNotification notification,
        string error)
    {
        const string sql = """
        UPDATE dbo.AppNotifications
        SET 
            RetryCount = ISNULL(RetryCount, 0) + 1,
            LastModified = @LastModified,
            LastModifiedBy = 'NotificationProcessor',
            Error = @Error
        WHERE Id = @Id
        """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                notification.Id,
                LastModified = dateTimeProvider.Now,
                Error = error
            },
            transaction: transaction);
    }
}
