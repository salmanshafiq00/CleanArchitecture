using Infrastructure.Identity.Entities;

namespace Infrastructure.Identity.Services;

internal interface ITokenProviderService
{
    Task<(string AccessToken, int ExpiresInMinutes)> GenerateAccessTokenAsync(AppUser user);
    string GenerateRefreshToken();

}
