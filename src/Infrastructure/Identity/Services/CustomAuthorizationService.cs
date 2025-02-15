using Application.Common.Abstractions.Identity;
using Application.Common.Constants;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity.Services;

public class CustomAuthorizationService(
    UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService,
    ILogger<CustomAuthorizationService> logger) : ICustomAuthorizationService
{
    public async Task<Result> AuthorizeAsync(string userId, string policyName, CancellationToken cancellation = default)
    {
        try
        {
            var user = await userManager.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == userId, cancellation);

            if (user is null)
            {
                logger.LogWarning("User not found for authorization. UserId: {UserId}", userId);
                return Error.NotFound(nameof(user), ErrorMessages.USER_NOT_FOUND);
            }

            var principal = await userClaimsPrincipalFactory.CreateAsync(user);

            var result = await authorizationService.AuthorizeAsync(principal, policyName);

            return result.Succeeded
                ? Result.Success()
                : Error.Unauthorized(nameof(ErrorType.Unauthorized), "User is not authorized for the requested policy.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during authorization for UserId: {UserId}, Policy: {PolicyName}", userId, policyName);
            return Error.Failure(ErrorMessages.InvalidOperation, "An internal error occurred during authorization.");
        }
    }

    public async Task<Result> IsInRoleAsync(string userId, string role, CancellationToken cancellation = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellation);

        if (user is null) return Error.NotFound(nameof(user), ErrorMessages.USER_NOT_FOUND);

        return await userManager.IsInRoleAsync(user, role)
            ? Result.Success()
            : Error.Forbidden(nameof(ErrorType.Forbidden), "You have no permission to access the resource");
    }
}


