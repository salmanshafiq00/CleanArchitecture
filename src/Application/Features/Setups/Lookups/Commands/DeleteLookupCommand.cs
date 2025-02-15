using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;
using Application.Common.Constants;
using Domain.Shared;

namespace Application.Features.Setups.Lookups.Commands;

public record DeleteLookupCommand(Guid Id) : ICacheInvalidatorCommand
{
    public string[] CacheKeys => [AppCacheKeys.Lookup];
}

internal sealed class DeleteLookupCommandHandler(
    IApplicationDbContext dbContext,
    IPublisher publisher)
    : ICommandHandler<DeleteLookupCommand>
{
    public async Task<Result> Handle(DeleteLookupCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Lookups.FindAsync(request.Id, cancellationToken);

        if (entity is null) return Result.Failure(Error.NotFound(nameof(entity), ErrorMessages.EntityNotFound));

        dbContext.Lookups.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
