using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;
using Application.Features.Admin.Roles.Models;

namespace Application.Features.Admin.Roles.Queries;

public record GetMenusByRoleQuery(string RoleId)
    : ICacheableQuery<RoleMenuModel>
{
    [JsonIgnore]
    public string CacheKey => $"Role_{RoleId}_Menus";

    public bool? AllowCache => true;

    public TimeSpan? Expiration => null;
}

internal sealed class GetMenusByRoleQueryHandler(IIdentityRoleService roleService)
    : IQueryHandler<GetMenusByRoleQuery, RoleMenuModel>
{
    public async Task<Result<RoleMenuModel>> Handle(GetMenusByRoleQuery request, CancellationToken cancellationToken)
    {
        return await roleService.GetRoleMenusAsync(request.RoleId, cancellationToken);
    }
}
