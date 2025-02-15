using Application.Features.Identity.Models;

namespace Application.Common.Abstractions.Identity;

public interface IAuthService
{
    Task<Result<AuthenticatedResponse>> LoginAsync(string username, string password, bool rememberMe = false, CancellationToken cancellation = default);
    Task<Result<AuthenticatedResponse>> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellation = default);
    Task<Result> Logout(string userId, string accessToken, string refreshToken, CancellationToken cancellation = default);
    Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken cancellation = default);
    Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellation = default);
    Task<Result> ResetPasswordAsync(string email, string password, string token, CancellationToken cancellation = default);

}
