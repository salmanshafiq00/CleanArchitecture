using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;
using Application.Features.Admin.AppUsers.Models;

namespace Application.Features.Admin.AppUsers.Queries;

public record GetAppUserProfileQuery(string UserId)
    : ICacheableQuery<AppUserModel>
{
    [JsonIgnore]
    public string CacheKey => $"{AppCacheKeys.AppUser}_{UserId}";

    public bool? AllowCache => true;

    public TimeSpan? Expiration => null;
}

internal sealed class GetAppUserProfileQueryHandler(IIdentityService identityService)
    : IQueryHandler<GetAppUserProfileQuery, AppUserModel>
{
    public async Task<Result<AppUserModel>> Handle(GetAppUserProfileQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId) || Guid.Parse(request.UserId) == Guid.Empty)
        {
            return new AppUserModel();
        }
        return await identityService.GetProfileAsync(request.UserId, cancellationToken).ConfigureAwait(false);
    }
}
