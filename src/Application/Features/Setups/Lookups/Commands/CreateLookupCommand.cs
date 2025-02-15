using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;
using Domain.Common;

namespace Application.Features.Setups.Lookups.Commands;

public record CreateLookupCommand(
    string Name,
    string Code,
    string Description,
    bool Status,
    Guid? ParentId = null) : ICacheInvalidatorCommand<Guid>
{
    [JsonIgnore]
    public string[] CacheKeys => [AppCacheKeys.Lookup];
}

internal sealed class CreateLookupQueryHandler(
    IApplicationDbContext dbContext)
    : ICommandHandler<CreateLookupCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateLookupCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Adapt<Lookup>();

        dbContext.Lookups.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(entity.Id);
    }
}
