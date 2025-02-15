using System.Transactions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Communication;
using Application.Common.Abstractions.Identity;
using Application.Common.BackgroundJobs;
using Application.Common.Constants;
using Application.Features.Admin.AppUsers.Commands;
using Application.Features.Admin.AppUsers.Models;
using Infrastructure.Identity;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Infrastructure.Identity.Services;

public class IdentityService(
    UserManager<AppUser> userManager,
    IdentityContext identityContext,
    IDistributedCacheService cacheService,
    ILogger<IdentityService> logger,
    IConfiguration configuration,
    IBackgroundJobService backgroundJobService,
    IEmailService emailService) : IIdentityService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IdentityContext _identityContext = identityContext;
    private readonly IDistributedCacheService _cacheService = cacheService;
    private readonly ILogger<IdentityService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;
    private readonly IBackgroundJobService _backgroundJobService = backgroundJobService;
    private readonly IEmailService _emailService = emailService;

    public async Task<Result<string>> CreateUserAsync(
        CreateAppUserCommand command,
        CancellationToken cancellation = default)
    {
        // Use TransactionScope to manage transactions across multiple operations
        using var transaction = new TransactionScope(
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            // Create the user using UserManager
            var user = new AppUser
            {
                UserName = command.Username,
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                IsActive = command.IsActive,
                PhotoUrl = command.PhotoUrl,
                PhoneNumber = command.PhoneNumber
            };

            var createUserResult = await _userManager.CreateAsync(user, command.Password);
            if (!createUserResult.Succeeded)
            {
                return createUserResult.ToApplicationResult<string>(string.Empty);
            }

            // Add roles if specified
            if (command.Roles?.Count > 0)
            {
                await _userManager.AddToRolesAsync(user, command.Roles);
                await _identityContext.SaveChangesAsync(cancellation);
            }

            // Mark transaction as complete
            transaction.Complete();

            return Result<string>.Success(user.Id);
        }
        catch (Exception ex)
        {
            // TransactionScope will automatically roll back if `Complete` is not called
            return Result.Failure<string>(
                Error.Failure(ErrorMessages.UNABLE_CREATE_USER, $"An error occurred: {ex.Message}")
            );
        }
    }



    //public async Task<Result<string>> CreateUserAsync(
    //    CreateAppUserCommand command,
    //    CancellationToken cancellation = default)
    //{
    //    // Atomic transaction scope
    //    using var transaction = await _identityContext.Database.BeginTransactionAsync(cancellation);

    //    try
    //    {
    //        // Create the user
    //        var user = new ApplicationUser
    //        {
    //            UserName = command.Username,
    //            Email = command.Email,
    //            FirstName = command.FirstName,
    //            LastName = command.LastName,
    //            IsActive = command.IsActive,
    //            PhotoUrl = command.PhotoUrl,
    //            PhoneNumber = command.PhoneNumber
    //        };

    //        var createUserResult = await _userManager.CreateAsync(user, command.Password);
    //        if (!createUserResult.Succeeded)
    //        {
    //            return createUserResult.ToApplicationResult<string>(string.Empty);
    //        }

    //        // Add roles to the user, if any
    //        if (command.Roles?.Count > 0)
    //        {

    //            _identityContext.UserRoles.AddRange(command.Roles.Select(x => new IdentityUserRole<string>
    //            {
    //                UserId = user.Id,
    //                RoleId = x
    //            }));

    //            await _identityContext.SaveChangesAsync(cancellation);
    //        }

    //        // Commit transaction
    //        await transaction.CommitAsync(cancellation);

    //        return Result<string>.Success(user.Id);
    //    }
    //    catch (Exception ex)
    //    {
    //        // Rollback on error
    //        await transaction.RollbackAsync(cancellation);
    //        return Result.Failure<string>(Error.Failure(ErrorMessages.UNABLE_CREATE_USER, $"An error occurred: {ex.Message}"));
    //    }
    //}

    public async Task<Result> UpdateUserAsync(
        UpdateAppUserCommand command,
        CancellationToken cancellation = default)
    {
        // Begin a transaction to ensure atomicity
        await using var transaction = await _identityContext.Database.BeginTransactionAsync(cancellation);

        try
        {
            // Find the user using UserManager
            var user = await _userManager.FindByIdAsync(command.Id);
            if (user == null)
                return Result.Failure(Error.Failure("User.Update", ErrorMessages.USER_NOT_FOUND));

            // Update user properties
            user.UserName = command.Username;
            user.Email = command.Email;
            user.FirstName = command.FirstName;
            user.LastName = command.LastName;
            user.IsActive = command.IsActive;
            user.PhoneNumber = command.PhoneNumber;

            // Update the user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result.Failure(Error.Failure("User.Update", string.Join(", ", updateResult.Errors.Select(e => e.Description))));

            // Update roles (if specified)
            if (command.Roles?.Count > 0)
            {
                var roleUpdateResult = await UpdateUserRolesAsync(command.Roles, user, cancellation);
                if (!roleUpdateResult.IsSuccess)
                    return roleUpdateResult; // Rollback transaction if role update fails
            }

            // Commit the transaction
            await transaction.CommitAsync(cancellation);
            return Result.Success();
        }
        catch (Exception ex)
        {
            // Rollback the transaction in case of errors
            await transaction.RollbackAsync(cancellation);
            return Result.Failure(Error.Failure("User.Update", $"An error occurred: {ex.Message}"));
        }
    }

    private async Task<Result> UpdateUserRolesAsync(
        List<string> roles,
        AppUser user,
        CancellationToken cancellation = default)
    {
        // Get current roles of the user
        var currentRoles = await _userManager.GetRolesAsync(user);

        // Determine roles to remove and add
        var rolesToRemove = currentRoles.Except(roles).ToList();
        var rolesToAdd = roles.Except(currentRoles).ToList();

        // Remove roles
        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                return Result.Failure(Error.Failure("User.UpdateRoles", $"Failed to remove roles: {errors}"));
            }
        }

        // Add roles
        if (rolesToAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                return Result.Failure(Error.Failure("User.UpdateRoles", $"Failed to add roles: {errors}"));
            }
        }

        return Result.Success();
    }

    public async Task<string?> GetUserNameAsync(string userId, CancellationToken cancellation = default)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .FirstAsync(u => u.Id == userId, cancellation);

        return user?.UserName;
    }


    public async Task<Result> UpdateUserBasicAsync(
      UpdateAppUserBasicCommand command,
      CancellationToken cancellation = default)
    {
        //var user = await _identityContext.Users
        //    .SingleOrDefaultAsync(u => u.Id == command.Id, cancellation)
        //    .ConfigureAwait(false);

        var user = await _userManager.FindByIdAsync(command.Id);

        if (user is null)
            return Result.Failure(Error.Failure("User.Update", ErrorMessages.USER_NOT_FOUND));

        user.Email = command.Email;
        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.PhoneNumber = command.PhoneNumber;

        var identityResult = await _userManager.UpdateAsync(user);

        if (!identityResult.Succeeded)
            return Result.Failure(Error.Failure("User.Update", ErrorMessages.UNABLE_UPDATE_USER));

        //var result = await _identityContext.SaveChangesAsync(cancellation);

        return identityResult.ToApplicationResult();
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken cancellation = default)
    {
        var user = await _userManager.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellation);

        if (user is null)
            return Result.Failure(Error.NotFound(nameof(user), ErrorMessages.USER_NOT_FOUND));

        // Step 1: Get the user's roles
        var userRoles = await _userManager.GetRolesAsync(user);

        // Step 2: Remove the user from roles
        var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
        if (!removeRolesResult.Succeeded)
        {
            return Result.Failure(Error.Failure("User.RemoveRoles", ErrorMessages.UNABLE_REMOVE_ROLES));
        }

        // Step 3: Delete the user
        var result = await _userManager.DeleteAsync(user);

        // Step 4: Return result
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Failure("User.Delete", ErrorMessages.UNABLE_DELETE_USER));
    }

    public async Task<Result> ChangePhotoAsync(
      string userId,
      string photoUrl,
      CancellationToken cancellation = default)
    {
        var user = await _identityContext.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellation)
            .ConfigureAwait(false);

        if (user is null)
            return Result.Failure(Error.Failure("User.Update", ErrorMessages.USER_NOT_FOUND));

        user.PhotoUrl = photoUrl;

        var result = await _identityContext.SaveChangesAsync(cancellation);

        return result > 0
            ? Result.Success()
            : Result.Failure(Error.Failure("User.Update", ErrorMessages.UNABLE_UPDATE_USER_PHOTO));
    }

    public async Task<Result<AppUserModel>> GetUserAsync(
      string id,
      CancellationToken cancellation = default)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id, cancellation);

        if (user is null)
            return Result.Failure<AppUserModel>(Error.Failure("User.Found", ErrorMessages.USER_NOT_FOUND));

        var appUser = new AppUserModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            PhotoUrl = user.PhotoUrl,
            Username = user.UserName!,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
        };

        appUser.Roles = await _userManager.GetRolesAsync(user);

        //appUser.Roles = await _identityContext.UserRoles
        //    .AsNoTracking()
        //    .Where(x => x.UserId == id)
        //    .Select(x => x.RoleId)
        //    .ToListAsync(cancellation);



        return Result.Success(appUser);
    }

    public async Task<Result<AppUserModel>> GetProfileAsync(
      string id,
      CancellationToken cancellation = default)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id, cancellation);

        if (user is null)
            return Result.Failure<AppUserModel>(Error.Failure("User.Found", ErrorMessages.USER_NOT_FOUND));

        var appUser = new AppUserModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            PhotoUrl = user.PhotoUrl,
            Username = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        };

        var roles = await _userManager.GetRolesAsync(user);
        appUser.AssignedRoles = string.Join(", ", roles);

        return Result.Success(appUser);
    }

    public async Task<Result<string[]>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _identityContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure<string[]>(Error.NotFound(nameof(userId), ErrorMessages.USER_NOT_FOUND));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var userRoles = _identityContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .AsQueryable();

        var rolePermissions = await _identityContext.RoleClaims
            .AsNoTracking()
            .Where(x => userRoles.Contains(x.RoleId))
            .Select(x => x.ClaimValue!)
            .ToArrayAsync(cancellationToken);

        return roles.Union(rolePermissions).ToArray();

    }

    public async Task<IDictionary<string, string?>> GetUsersByRole(
        string roleName,
        CancellationToken cancellation = default)
    {
        var result = await _userManager.GetUsersInRoleAsync(roleName);

        return result?.ToDictionary(x => x.UserName, y => $"{y.FirstName} {y.LastName}")!;
    }

    public async Task<Result> AddToRolesAsync(
        string userId,
        List<string> roleNames,
        CancellationToken cancellation = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null) return Result.Failure(Error.NotFound(nameof(user), ErrorMessages.USER_NOT_FOUND));

        var result = await _userManager.AddToRolesAsync(user, roleNames);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(Error.Unauthorized(nameof(ErrorType.Unauthorized), string.Empty));
    }

    private async Task DeleteAndAddUserRoles(
    List<string> roles,
    AppUser user,
    CancellationToken cancellation)
    {
        await _identityContext.UserRoles
                    .Where(x => x.UserId == user.Id)
                    .ExecuteDeleteAsync(cancellation);

        _identityContext.UserRoles
            .AddRange(roles.Select(x => new IdentityUserRole<string>
            {
                UserId = user.Id,
                RoleId = x
            }));
    }
}
