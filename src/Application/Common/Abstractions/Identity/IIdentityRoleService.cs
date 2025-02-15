using Application.Common.Models;
using Application.Features.Admin.Roles.Models;

namespace Application.Common.Abstractions.Identity;

public interface IIdentityRoleService
{
    Task<Result<string>> CreateRoleAsync(string name, List<Guid> rolemenus, List<string> permissions, CancellationToken cancellation = default);
    Task<Result> UpdateRoleAsync(string id, string name, List<Guid> rolemenus, List<string> permissions, CancellationToken cancellation = default);
    Task<Result<RoleModel>> GetRoleAsync(string id, CancellationToken cancellation = default);
    Task<Result> DeleteRoleAsync(string id, CancellationToken cancellation = default);
    Task<Result> AddOrRemoveClaimsToRoleAsync(string roleId, List<string> permissions, CancellationToken cancellation = default);
    Result<IList<DynamicTreeNodeModel>> GetAllPermissions();
    Task<Result<RolePermissionModel>> GetRolePermissionsAsync(string roleId, CancellationToken cancellation = default);
    Task<Result<RoleMenuModel>> GetRoleMenusAsync(string roleId, CancellationToken cancellation = default);
    Task<Result> RemoveAndAddRoleMenuAsync(string roleId, List<Guid> roleMenus, CancellationToken cancellationToken = default);
}
