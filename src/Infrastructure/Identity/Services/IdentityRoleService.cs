using System.Transactions;
using Application.Common.Abstractions;
using Application.Common.Abstractions.Identity;
using Application.Common.Constants;
using Application.Common.Models;
using Application.Features.Admin.Roles.Models;
using Domain.Admin;
using Infrastructure.Identity;
using Infrastructure.Identity.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity.Services;

internal class IdentityRoleService(
    RoleManager<IdentityRole> roleManager,
    IdentityContext identityContext,
    IApplicationDbContext appDbContext,
    ILogger<IdentityRoleService> logger)
    : IIdentityRoleService
{
    public async Task<Result<string>> CreateRoleAsync(
        string name,
        List<Guid> rolemenus,
        List<string> permissions,
        CancellationToken cancellation = default)
    {

        try
        {
            var role = new IdentityRole
            {
                Name = name,
                NormalizedName = name.ToUpper()
            };

            var identityRestult = await roleManager.CreateAsync(role);

            return identityRestult.ToApplicationResult(role.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured to save role");
            return Result.Failure<string>(Error.Failure("Role.Create", "Error occured to save role."));
        }
    }

    //public async Task<Result<string>> CreateRoleAsync(
    //    string name,
    //    List<Guid> rolemenus,
    //    List<string> permissions,
    //    CancellationToken cancellation = default)
    //{
    //    using var transaction = await identityContext.Database.BeginTransactionAsync(cancellation);

    //    try
    //    {
    //        var role = new IdentityRole
    //        {
    //            Name = name,
    //            NormalizedName = name.ToUpper()
    //        };

    //        await identityContext.Roles.AddAsync(role, cancellation);
    //        await identityContext.SaveChangesAsync(cancellation);

    //        foreach (var appmenuId in rolemenus ?? [])
    //        {
    //            appDbContext.RoleMenus.Add(new RoleMenu
    //            {
    //                RoleId = role.Id,
    //                AppMenuId = appmenuId,
    //            });
    //        }

    //        foreach (var permission in permissions)
    //        {
    //            identityContext.RoleClaims.Add(new IdentityRoleClaim<string>
    //            {
    //                RoleId = role.Id,
    //                ClaimType = CustomClaimTypes.Permission,
    //                ClaimValue = permission
    //            });
    //        }

    //        await appDbContext.SaveChangesAsync(cancellation);
    //        await identityContext.SaveChangesAsync(cancellation);

    //        await transaction.CommitAsync(cancellation);

    //        return Result.Success(role.Id);
    //    }
    //    catch (Exception ex)
    //    {
    //        await transaction.RollbackAsync(cancellation);
    //        logger.LogError(ex, "Error occured to save role");
    //        return Result.Failure<string>(Error.Failure("Role.Create", "Error occured to save role."));
    //    }
    //}

    public async Task<Result> UpdateRoleAsync(
        string id,
        string name,
        List<Guid> rolemenus,
        List<string> permissions,
        CancellationToken cancellation = default)
    {
        try
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role is null)
                Result.Failure(Error.Failure("Role.Update", ErrorMessages.ROLE_NOT_FOUND));

            role.Name = name;
            role.NormalizedName = name.ToUpper();

            var identityResult = await roleManager.UpdateAsync(role);

            return identityResult.ToApplicationResult();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occured to update role");
            return Result.Failure<string>(Error.Failure("Role.Update", "Error occured to update role."));

        }
    }

    //public async Task<Result> UpdateRoleAsync(
    //    string id,
    //    string name,
    //    List<Guid> rolemenus,
    //    List<string> permissions,
    //    CancellationToken cancellation = default)
    //{
    //    using var transaction = await identityContext.Database.BeginTransactionAsync(cancellation);

    //    try
    //    {
    //        var role = await identityContext.Roles.FindAsync(id, cancellation);

    //        if (role is null)
    //            Result.Failure(Error.Failure("Role.Update", ErrorMessages.ROLE_NOT_FOUND));

    //        role!.Name = name;
    //        role!.NormalizedName = name.ToUpper();

    //        await RemoveAndAddPermissionAsync(role!, permissions, cancellation);
    //        await RemoveAndAddRoleMenuAsync(role!, rolemenus, cancellation);

    //        await identityContext.SaveChangesAsync(cancellation);
    //        await appDbContext.SaveChangesAsync(cancellation);

    //        await transaction.CommitAsync(cancellation);

    //        return Result.Success();

    //    }
    //    catch (Exception ex)
    //    {
    //        await transaction.RollbackAsync(cancellation);
    //        logger.LogError(ex, "Error occured to update role");
    //        return Result.Failure<string>(Error.Failure("Role.Create", "Error occured to update role."));

    //    }
    //}

    public async Task<Result> DeleteRoleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        // Use using statement to ensure proper disposal
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            // Find the role
            var role = await roleManager.FindByIdAsync(id);
            if (role is null)
                return Result.Failure(Error.Failure("Role.Delete", ErrorMessages.ROLE_NOT_FOUND));

            // Remove all claims associated with this role first
            var roleClaims = await roleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims)
            {
                var removeClaimResult = await roleManager.RemoveClaimAsync(role, claim);
                if (!removeClaimResult.Succeeded)
                {
                    return Result.Failure(Error.Failure("Role.Delete",
                        $"Failed to remove role claim: {claim.Type}"));
                }
            }

            // Delete the role
            var deleteResult = await roleManager.DeleteAsync(role);
            if (!deleteResult.Succeeded)
            {
                return Result.Failure(Error.Failure("Role.Delete", ErrorMessages.UNABLE_DELETE_ROLE));
            }

            // Complete the transaction
            transaction.Complete();
            return Result.Success();
        }
        catch (Exception ex)
        {
            // Log the exception if you have a logger
            // _logger.LogError(ex, "Error deleting role with ID {RoleId}", id);

            return Result.Failure(Error.Failure("Role.Delete",
                "An unexpected error occurred while deleting the role"));
        }
    }

    public async Task<Result<RoleModel>> GetRoleAsync(
        string id,
        CancellationToken cancellation = default)
    {
        var role = await roleManager.FindByIdAsync(id);

        if (role is null)
            Result.Failure<RoleModel>(Error.Failure("Role.Delete", ErrorMessages.ROLE_NOT_FOUND));

        var permissions = await roleManager.GetClaimsAsync(role);

        var roleMenus = await appDbContext.RoleMenus
            .AsNoTracking()
            .Where(x => x.RoleId.ToLower() == id.ToLower())
            .Select(x => x.AppMenuId)
            .ToListAsync(cancellation);

        return Result.Success(new RoleModel
        {
            Id = role!.Id,
            Name = role.Name!,
            RoleMenus = roleMenus,
            Permissions = permissions?.Select(x => x.Value).ToList()
        });
    }

    public async Task<Result<RolePermissionModel>> GetRolePermissionsAsync(
        string roleId,
        CancellationToken cancellation = default)
    {
        var role = await roleManager.FindByIdAsync(roleId);

        if (role is null)
            Result.Failure<RoleModel>(Error.Failure("Role.Delete", ErrorMessages.ROLE_NOT_FOUND));

        var permissions = await roleManager.GetClaimsAsync(role!);

        return Result.Success(new RolePermissionModel
        {
            RoleId = role!.Id,
            RoleName = role!.Name,
            Permissions = permissions?.Select(x => x.Value).ToList()
        });
    }

    public async Task<Result<RoleMenuModel>> GetRoleMenusAsync(
        string roleId,
        CancellationToken cancellation = default)
    {
        var role = await roleManager.FindByIdAsync(roleId);

        if (role is null)
            Result.Failure<RoleModel>(Error.Failure("Role.Delete", ErrorMessages.ROLE_NOT_FOUND));

        var roleMenus = await appDbContext.RoleMenus
            .AsNoTracking()
            .Where(x => x.RoleId.ToLower() == roleId.ToLower())
            .Select(x => x.AppMenuId)
            .ToListAsync(cancellation);

        return Result.Success(new RoleMenuModel
        {
            RoleId = role!.Id,
            RoleName = role.Name,
            RoleMenus = roleMenus
        });
    }

    public async Task<Result> AddOrRemoveClaimsToRoleAsync(
        string roleId,
        List<string> permissions,
        CancellationToken cancellation = default)
    {
        var role = await roleManager.FindByIdAsync(roleId);

        if (role is null)
            Result.Failure(Error.Failure("Role.Update", ErrorMessages.ROLE_NOT_FOUND));

        var result = await RemoveAndAddPermissionAsync(role!, permissions, cancellation);

        return result;
    }

    public Result<IList<DynamicTreeNodeModel>> GetAllPermissions()
    {
        return Result.Success(PermissionHelper.MapPermissionsToTree());
    }

    public async Task<Result> RemoveAndAddRoleMenuAsync(
        string roleId,
        List<Guid> roleMenus,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role is null)
                Result.Failure(Error.Failure("Role.Update", ErrorMessages.ROLE_NOT_FOUND));

            var result = await RemoveAndAddRoleMenuAsync(role!, roleMenus, cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fail to update RoleMenu");
            return Result.Failure(Error.Failure("Role.Menu", ErrorMessages.UNABLE_UPDATE_ROLE_MENU));
        }
    }

    private async Task<Result> RemoveAndAddPermissionAsync(
        IdentityRole role,
        List<string> permissions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existedPermissions = await identityContext.RoleClaims
            .Where(x => x.RoleId == role.Id)
            .ToListAsync(cancellationToken);

            identityContext.RoleClaims.RemoveRange(existedPermissions);

            var newPermissions = permissions.Select(x => new IdentityRoleClaim<string>
            {
                RoleId = role.Id,
                ClaimType = CustomClaimTypes.Permission,
                ClaimValue = x
            });

            identityContext.RoleClaims.AddRange(newPermissions);

            await identityContext.SaveChangesAsync(cancellationToken);

            return Result.Success();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fail to update role permission");
            return Result.Failure(Error.Failure("Role.Permission", "Fail to update Role's Permissions"));
        }
    }

    private async Task<Result> RemoveAndAddRoleMenuAsync(
        IdentityRole role,
        List<Guid> rolemenus,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existedRoleMenus = await appDbContext.RoleMenus
            .Where(x => x.RoleId == role.Id)
            .ToListAsync(cancellationToken);

            appDbContext.RoleMenus.RemoveRange(existedRoleMenus);

            foreach (var appmenuId in rolemenus ?? [])
            {
                appDbContext.RoleMenus.Add(new RoleMenu
                {
                    RoleId = role.Id,
                    AppMenuId = appmenuId,
                });
            }
            await appDbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fail to update RoleMenu");
            return Result.Failure(Error.Failure("Role.Menus", "Fail to update Role's Menus"));
        }
    }
}
