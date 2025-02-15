using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;
using Domain.Common;
using Domain.Shared;
using Mapster;

namespace Application.Features.Setups.LookupDetails.Commands;

public record CreateLookupDetailCommand(
    string Name,
    string Code,
    string Description,
    bool Status,
    Guid LookupId,
    Guid? ParentId = null) : ICacheInvalidatorCommand<Guid>
{
    public string[] CacheKeys => [AppCacheKeys.LookupDetail];
}

internal sealed class CreateLookupDetailCommandHandler(
    IApplicationDbContext dbContext)
    : ICommandHandler<CreateLookupDetailCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateLookupDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Adapt<LookupDetail>();

        dbContext.LookupDetails.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(entity.Id);
    }
}
