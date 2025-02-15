using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;
using Application.Common.Constants;

namespace Application.Features.Setups.LookupDetails.Commands;

public record DeleteLookupDetailCommand(Guid Id) : ICacheInvalidatorCommand
{
    public string[] CacheKeys => [AppCacheKeys.LookupDetail];
}

internal sealed class DeleteLookupDetailCommandHandler(
    IApplicationDbContext dbContext)
    : ICommandHandler<DeleteLookupDetailCommand>
{
    public async Task<Result> Handle(DeleteLookupDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.LookupDetails.FindAsync(request.Id, cancellationToken);

        if (entity is null) return Result.Failure(Error.NotFound(nameof(entity), ErrorMessages.EntityNotFound));

        dbContext.LookupDetails.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
