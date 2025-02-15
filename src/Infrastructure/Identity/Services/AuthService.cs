using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Abstractions;
using Application.Common.Abstractions.Communication;
using Application.Common.Abstractions.Identity;
using Application.Common.BackgroundJobs;
using Application.Common.Constants;
using Application.Features.Identity.Models;
using Infrastructure.Identity.BackgroundJobs;
using Infrastructure.Identity;
using Infrastructure.Identity.Entities;
using Infrastructure.Identity.OptionsSetup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Services;

internal sealed class AuthService(
    UserManager<AppUser> userManager,
    ITokenProviderService tokenProvider,
    IdentityContext dbContext,
    IOptionsSnapshot<JwtOptions> jwtOptions,
    ILogger<AuthService> logger,
    IBackgroundJobService backgroundJobService,
    IEmailService emailService,
    IConfiguration configuration,
    IHttpContextAccessor httpContext)
    : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<Result<AuthenticatedResponse>> LoginAsync(
        string username,
        string password,
        bool rememberMe = false,
        CancellationToken cancellation = default)
    {
        var user = await FindUserByUsernameOrEmail(username);

        if (user is null
            || !await userManager.CheckPasswordAsync(user, password))
        {
            logger.LogWarning("Invalid login attempt for user {Username}", username);
            return Error.NotFound(nameof(user), ErrorMessages.WRONG_USERNAME_PASSWORD);
        }

        // Generate new tokens
        return await GenerateTokenResponseAsync(user, rememberMe, cancellation);
    }

    public async Task<Result<AuthenticatedResponse>> RefreshTokenAsync(
        string accessToken,
        string refreshToken,
        CancellationToken cancellation = default)
    {
        var existingRefreshToken = await dbContext.RefreshTokens
            .Include(x => x.ApplicationUser)
            .SingleOrDefaultAsync(x => x.Token == refreshToken, cancellation);

        if (existingRefreshToken is null
            || !existingRefreshToken.IsActive)
        {
            logger.LogWarning("Token refresh failed: Invalid or inactive refresh token");
            return Error.Validation("Token.Invliad", ErrorMessages.TOKEN_INVALID_OR_EXPIRED);
        }

        // Get ClaimPrincipal from accessToken
        var claimsPrincipalResult = GetClaimsPrincipalFromToken(accessToken);

        if (claimsPrincipalResult.IsFailure)
            return claimsPrincipalResult.Error;

        var userId = claimsPrincipalResult.Value?.FindFirstValue(ClaimTypes.NameIdentifier);

        // Ensure the refresh token belongs to the user
        if (existingRefreshToken.UserId != userId)
        {
            logger.LogWarning("Token refresh failed: Token does not belong to user");
            return Error.Validation("Token", "Invalid refresh token");
        }

        var (newAccessToken, accessTokenExpiration) = await tokenProvider
            .GenerateAccessTokenAsync(existingRefreshToken.ApplicationUser);

        var newRefreshToken = await RotateRefreshToken(existingRefreshToken, cancellation);

        return new AuthenticatedResponse(
            newAccessToken,
            accessTokenExpiration,
            newRefreshToken.Token,
            newRefreshToken.Expires);
    }

    public async Task<Result> Logout(
        string userId,
        string accessToken,
        string refreshToken,
        CancellationToken cancellation = default)
    {
        // Get ClaimPrincipal from accessToken
        var claimsPrincipalResult = GetClaimsPrincipalFromToken(accessToken);

        // Get Identity UserId  from ClaimPrincipal
        var userIdFromAccessToken = claimsPrincipalResult.Value?.FindFirstValue(ClaimTypes.NameIdentifier);

        var existingRefreshToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(x => x.Token == refreshToken, cancellation);

        if (existingRefreshToken is not null)
        {
            existingRefreshToken.Revoked = DateTime.Now;
            await dbContext.SaveChangesAsync(cancellation);
        }
        logger.LogInformation("User {UserId} logged out successfully", userId);

        return Result.Success("Logout Successfully");
    }

    public async Task<Result> ChangePasswordAsync(
         string userId,
         string currentPassword,
         string newPassword,
         CancellationToken cancellation = default)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Error.Failure("User.Update", ErrorMessages.USER_NOT_FOUND);

        var identityResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!identityResult.Succeeded)
            return identityResult.ToApplicationResult();

        // Invalidate all refresh tokens
        await InvalidateUserAllRefreshTokensAsync(userId, cancellation);

        return identityResult.ToApplicationResult("Password changed successfully.");
    }

    public async Task<Result> ForgotPasswordAsync(
            string email,
            CancellationToken cancellation = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        //if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        if (user is null)
            return Result.Success("If the email is registered, a reset link will be sent.");

        // Generate password reset token
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);

        var clientUrl = configuration.GetValue<string>("ClientUrl");

        // Create the reset link (frontend reset password route)
        var resetLink = $"{clientUrl}/auth/reset-password?token={encodedToken}&email={email}";

        // Enqueue email sending as a background job
        backgroundJobService.EnqueueJob(() =>
            SendPasswordResetEmail(user, resetLink));

        return Result.Success("Password reset email sent successfully.");
    }

    public async Task<Result> ResetPasswordAsync(
        string email,
        string password,
        string token, CancellationToken cancellation = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            logger.LogWarning("Password reset requested for unknown user.");
            return Error.Failure("User.ResetPassword", ErrorMessages.USER_NOT_FOUND);
        }

        var result = await userManager.ResetPasswordAsync(user, token, password);

        if (!result.Succeeded)
            return result.ToApplicationResult("Password reset failed.");

        // Invalidate all refresh tokens
        await InvalidateUserAllRefreshTokensAsync(user.Id, cancellation);

        // Optionally, send a confirmation email
        backgroundJobService.EnqueueJob(() =>
            PasswordResetConfirmationEmail(user));

        return Result.Success("Password reset successfully.");
    }

    private async Task<Result<AuthenticatedResponse>> GenerateTokenResponseAsync(
        AppUser user,
        bool rememberMe = false,
        CancellationToken cancellationToken = default)
    {
        var (accessToken, accessTokenExpiration) = await tokenProvider.GenerateAccessTokenAsync(user);

        var refreshToken = new RefreshToken
        {
            Token = tokenProvider.GenerateRefreshToken(),
            Expires = rememberMe ? DateTime.Now.AddDays(_jwtOptions.RememberMe) : DateTime.Now.AddDays(_jwtOptions.RefreshTokenExpires),
            Created = DateTime.Now,
            CreatedByIp = GetIpAddress(),
            UserId = user.Id
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthenticatedResponse(
            accessToken,
            accessTokenExpiration,
            refreshToken.Token,
            refreshToken.Expires));
    }

    private async Task<RefreshToken> RotateRefreshToken(
        RefreshToken existingToken,
        CancellationToken cancellationToken = default)
    {
        var newRefreshToken = new RefreshToken
        {
            Token = tokenProvider.GenerateRefreshToken(),
            Expires = DateTime.Now.AddDays(_jwtOptions.RefreshTokenExpires),
            Created = DateTime.Now,
            CreatedByIp = GetIpAddress(),
            UserId = existingToken.UserId
        };
        existingToken.Revoked = DateTime.Now;

        dbContext.RefreshTokens.Add(newRefreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return newRefreshToken;
    }

    private async Task<AppUser> FindUserByUsernameOrEmail(string identifier)
    {
        return await userManager.FindByEmailAsync(identifier)
               ?? await userManager.FindByNameAsync(identifier);
    }

    private async Task InvalidateUserAllRefreshTokensAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        // Delete all refresh tokens for the user
        await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

        logger.LogInformation("Deleted all refresh tokens for user {UserId}", userId);
    }

    // Method to handle email sending (non-async signature)
    private void SendPasswordResetEmail(AppUser user, string resetLink)
    {
        try
        {
            var templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Templates",
                "ForgotPassword",
                 "ForgotPassword.cshtml");

            emailService.SendTemplateEmailAsync(
                user.Email,
                "Password Reset",
                new { ReceiverName = user.FirstName, ResetLink = resetLink },
                templatePath
            ).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending password reset email to {Email}", user.Email);
        }
    }

    // Method to handle email sending (non-async signature)
    private void PasswordResetConfirmationEmail(AppUser user)
    {
        try
        {
            var clientUrl = configuration.GetValue<string>("ClientUrl");

            var templatePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Templates",
                "ForgotPassword",
                "ResetPasswordConfirmation.cshtml");

            emailService.SendTemplateEmailAsync(
                user.Email,
                "Password Reset",
                new { ReceiverName = user.FirstName, SiteLink = clientUrl },
                templatePath
            ).GetAwaiter().GetResult();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending password reset confirmation email to {Email}", user.Email);
        }
    }

    private string? GetIpAddress()
    {
        return httpContext?.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }

    private Result<ClaimsPrincipal> GetClaimsPrincipalFromToken(string accessToken)
    {

        try
        {
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // it's false because already token lifetime validated
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return Error.Validation("Token", ErrorMessages.INVALID_TOKEN);
            }

            return principal;
        }
        catch
        {
            return Error.Validation("Token", ErrorMessages.INVALID_TOKEN);
        }
    }
}

