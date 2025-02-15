using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;
using Application.Features.Admin.Roles.Models;

namespace Application.Features.Admin.Roles.Queries;

public record GetPermissionsByRoleQuery(string RoleId)
    : ICacheableQuery<RolePermissionModel>
{
    [JsonIgnore]
    public string CacheKey => $"Role_{RoleId}_Permissions";

    public bool? AllowCache => false;

    public TimeSpan? Expiration => null;
}

internal sealed class GetPermissionsByRoleQueryHandler(IIdentityRoleService roleService)
    : IQueryHandler<GetPermissionsByRoleQuery, RolePermissionModel>
{
    public async Task<Result<RolePermissionModel>> Handle(GetPermissionsByRoleQuery request, CancellationToken cancellationToken)
    {
        return await roleService.GetRolePermissionsAsync(request.RoleId, cancellationToken);
    }
}
