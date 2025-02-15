using Application.Common.Abstractions;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Admin.AppNotifications.Commands;

public record MarkSeenUserNotificationCommand : ICommand;

internal sealed class MarkSeenUserNotificationCommandHandler(
    ISqlConnectionFactory sqlConnection,
    IUser user)
    : ICommandHandler<MarkSeenUserNotificationCommand>
{
    public async Task<Result> Handle(MarkSeenUserNotificationCommand request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();
        var sql = $"""
            UPDATE [dbo].AppNotifications
            SET IsSeen = 1
            WHERE RecieverId = @UserId
            """;
        await connection.ExecuteAsync(sql, new { UserId = user.Id });

        return Result.Success();
    }
}
