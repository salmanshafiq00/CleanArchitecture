using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Infrastructure.Identity.OptionsSetup;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Services;

internal sealed class AccessTokenValidator(
    JwtSecurityTokenHandler tokenHandler,
    IOptions<JwtOptions> options)
    : IAccessTokenValidator
{
    private readonly JwtOptions _jwtOptions = options.Value;

    public async Task<TokenValidationResult> ValidateTokenAsync(string accessToken)
    {
        TokenValidationParameters tokenParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        return await tokenHandler.ValidateTokenAsync(accessToken, tokenParameters);
    }
}
