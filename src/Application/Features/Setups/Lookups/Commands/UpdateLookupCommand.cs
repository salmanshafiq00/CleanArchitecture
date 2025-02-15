using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;
using Application.Common.Constants;
using Domain.Common.DomainEvents;

namespace Application.Features.Setups.Lookups.Commands;

public record UpdateLookupCommand(
    Guid Id,
    string Name,
    string Code,
    string Description,
    bool Status,
    Guid? ParentId) : ICacheInvalidatorCommand
{
    public string[] CacheKeys => [AppCacheKeys.Lookup, AppCacheKeys.LookupDetail];
}

internal sealed class UpdateLookupCommandHandler(
    IApplicationDbContext dbContext,
    IPublisher publisher,
    IUser user)
    : ICommandHandler<UpdateLookupCommand>
{
    public async Task<Result> Handle(UpdateLookupCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Lookups.FindAsync(request.Id, cancellationToken);

        if (entity is null) return Error.NotFound("Lookup.NotFound", ErrorMessages.EntityNotFound);

        bool oldStatus = entity.Status;

        entity.Name = request.Name;
        entity.Code = request.Code;
        entity.Description = request.Description;
        entity.Status = request.Status;
        entity.ParentId = request.ParentId;

        entity.AddDomainEvent(new LookupUpdatedEvent(entity));
        // Add notification created event 
        //entity.AddDomainEvent(new NotificationCreatedEvent(new AppNotification
        //{
        //    SenderId = user.Id,
        //    RecieverId = user.Id,
        //    Title = "Lookup Updated",
        //    Description = $"Lookup {entity.Name} has been updated",
        //    Url = $"/lookups/{entity.Id}",
        //}));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
