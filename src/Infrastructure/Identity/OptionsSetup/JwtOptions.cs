namespace Infrastructure.Identity.OptionsSetup;

public class JwtOptions
{
    public const string JWT = nameof(JWT);

    public required string SecretKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required int DurationInMinutes { get; init; }
    public required int RefreshTokenExpires { get; init; }
    public required int RememberMe { get; init; }
}
