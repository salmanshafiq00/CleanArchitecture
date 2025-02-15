using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Services;

internal interface IAccessTokenValidator
{
    Task<TokenValidationResult> ValidateTokenAsync(string accessToken);
}
