namespace Infrastructure.Identity.BackgroundJobs;

internal interface IRefreshTokenCleanup
{
    Task CleanupExpiredTokensAsync();
}
