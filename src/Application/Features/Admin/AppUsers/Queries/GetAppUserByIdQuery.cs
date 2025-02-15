using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;
using Application.Features.Admin.AppUsers.Models;

namespace Application.Features.Admin.AppUsers.Queries;

public record GetAppUserByIdQuery(string Id)
    : ICacheableQuery<AppUserModel>
{
    [JsonIgnore]
    public string CacheKey => $"{AppCacheKeys.AppUser}:{Id}";

    public bool? AllowCache => false;

    public TimeSpan? Expiration => null;
}

internal sealed class GetAppUserByIdQueryHandler(IIdentityService identityService)
    : IQueryHandler<GetAppUserByIdQuery, AppUserModel>
{
    public async Task<Result<AppUserModel>> Handle(GetAppUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Id) || Guid.Parse(request.Id) == Guid.Empty)
        {
            return new AppUserModel();
        }
        return await identityService.GetUserAsync(request.Id, cancellationToken).ConfigureAwait(false);
    }
}
