using System.Data;
using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Events;
using Domain.Admin;
using Domain.Common;
using Domain.Common.DomainEvents;

namespace Application.Features.Setups.LookupDetails.EventHandlers;

internal sealed class LookupUpdatedEventHandler(
    ISqlConnectionFactory sqlConnection,
    IPublisher publisher,
    ILogger<LookupUpdatedEventHandler> logger,
    IUser user) : INotificationHandler<LookupUpdatedEvent>
{
    public async Task Handle(LookupUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.CreateNewConnection();
        using var transaction = connection.BeginTransaction();
        try
        {
            // 1. Update Lookup Details
            var sql = """
                UPDATE dbo.LookupDetails
                SET Status = @Status,
                    LastModifiedBy = @UserId,
                    LastModified = @Now
                WHERE LookupId = @LookupId
                """;

            var result = await connection.ExecuteAsync(
                sql,
                new
                {
                    notification.Lookup.Status,
                    LookupId = notification.Lookup.Id,
                    UserId = user.Id,
                    DateTime.Now
                },
                transaction: transaction);

            // 2. Create Notification if update was successful
            if (result > 0)
            {

                // 3. Invalidate Cache
                await publisher.Publish(
                    new CacheInvalidationEvent { CacheKeys = [AppCacheKeys.LookupDetail] },
                    cancellationToken);
            }

            await CreateNotification(notification.Lookup, connection, transaction);

            logger.LogInformation(
                "Event Handler: {EventHandlerName} processed lookup {LookupId} at {ExecutedOn}",
                nameof(LookupUpdatedEventHandler),
                notification.Lookup.Id,
                DateTime.UtcNow);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            logger.LogError(ex, "Error in LookupUpdatedEventHandler for lookup {LookupId}", notification.Lookup.Id);
            throw;
        }
    }

    private async Task CreateNotification(
        Lookup lookup,
        IDbConnection connection,
        IDbTransaction transaction)
    {
        var notificationSql = """
            INSERT INTO dbo.AppNotifications
            (Title, Description, SenderId, RecieverId, Url, CreatedBy, Created)
            VALUES
            (@Title, @Description, @SenderId, @RecieverId, @Url, @CreatedBy, @Created)
            """;

        var notification = new
        {
            Title = "Lookup Updated",
            Description = $"Lookup '{lookup.Name}' has been updated",
            SenderId = lookup.LastModifiedBy,
            RecieverId = lookup.LastModifiedBy,
            Url = $"/lookups/{lookup.Id}",
            //Type = NotificationType.Information,    // General info notification
            //Priority = NotificationPriority.Normal, // Regular priority
            CreatedBy = user.Id,
            Created = DateTime.Now
        };

        await connection.ExecuteAsync(
            notificationSql,
            notification,
            transaction: transaction);
    }
}
