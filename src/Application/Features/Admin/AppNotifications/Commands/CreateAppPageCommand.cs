using Domain.Admin;

namespace Application.Features.Admin.AppNotifications.Commands;

public record CreateAppNotificationCommand(
    string Title,
    string SubTitle,
    string ComponentName,
    string AppNotificationLayout) : ICacheInvalidatorCommand<Guid>
{
    public string[] CacheKeys => [AppCacheKeys.AppNotification];
}

internal sealed class CreateAppNotificationCommandHandler(IApplicationDbContext dbContext)
    : ICommandHandler<CreateAppNotificationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateAppNotificationCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Adapt<AppNotification>();
        dbContext.AppNotifications.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(entity.Id);
    }
}
