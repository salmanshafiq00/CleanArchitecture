using Application.Common.Abstractions.Identity;

namespace Application.Features.Identity.Queries;

public record GetUserPermissionsQuery(string UserId, bool IsCacheAllow = true) : ICacheableQuery<string[]>
{
    public string CacheKey => $"{AppCacheKeys.Role_Permissions}_{UserId}";

    public TimeSpan? Expiration => null;

    public bool? AllowCache => IsCacheAllow;
}

internal sealed class GetUserPermissionsQueryHandler(IIdentityService identityService) : IQueryHandler<GetUserPermissionsQuery, string[]>
{
    public async Task<Result<string[]>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await identityService.GetUserPermissionsAsync(request.UserId, cancellationToken);
    }
}
