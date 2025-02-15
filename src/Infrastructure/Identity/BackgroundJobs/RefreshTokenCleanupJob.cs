using System.Data;
using Dapper;
using Application.Common.Abstractions;

namespace Infrastructure.Identity.BackgroundJobs;

public class RefreshTokenCleanupJob(
    ISqlConnectionFactory sqlConnectionFactory,
    ILogger<RefreshTokenCleanupJob> logger)
    : IRefreshTokenCleanup
{
    public async Task CleanupExpiredTokensAsync()
    {
        try
        {
            logger.LogInformation("Beginning to process CleanupExpiredTokens");

            using IDbConnection connection = sqlConnectionFactory.GetOpenConnection();

            var cutoffDate = DateTime.Now.AddDays(-30);

            var sql = """
                DELETE [identity].RefreshTokens 
                WHERE Expires < @cutoffDate OR Revoked is not null
                """;

            var count = await connection.ExecuteAsync(sql, new { cutoffDate });

            logger.LogInformation("Cleaned up {count} expired tokens", count);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token cleanup");
        }
    }
}

