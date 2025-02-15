using Application.Common.BackgroundJobs;
using Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.SqlServer;
using Infrastructure.Communications;
using Infrastructure.Identity.BackgroundJobs;
using Infrastructure.Persistence.Outbox;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.BackgroundJobs;

public static class HangfireServiceExtension
{
    private const string DefaultConnection = nameof(DefaultConnection);

    public static IServiceCollection AddHangfireService(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnection);
        Guard.Against.Null(connectionString, message: $"Connection string '{nameof(DefaultConnection)}' not found.");

        // Configure SQL Server storage for Hangfire
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,

                // Add job expiration configuration
                JobExpirationCheckInterval = TimeSpan.FromHours(1), // Check for expired jobs hourly
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
            })
            .UseFilter(new ExpireFailedJobsAttribute(TimeSpan.FromDays(10))));

        // Add the Hangfire server
        services.AddHangfireServer(options =>
        {
            options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
            options.ServerTimeout = TimeSpan.FromMinutes(5);
        });

        // Jobs
        services.AddScoped<IBackgroundJobService, BackgroundJobService>();
        services.AddScoped<IProcessOutboxMessagesJob, ProcessOutboxMessagesJob>();
        services.AddScoped<IRefreshTokenCleanup, RefreshTokenCleanupJob>();


        return services;
    }

    public static WebApplication UseHangfire(this WebApplication app)
    {
        // Use Hangfire Dashboard (optional, can be secured)
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            //Authorization = [new HangfireDashboardAuthorizationFilter()],
            DarkModeEnabled = true,
        });

        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

        // Process outbox messages
        recurringJobManager.AddOrUpdate<IProcessOutboxMessagesJob>(
            "outbox-message-processor",
            job => job.ProcessOutboxMessagesAsync(),
            app.Configuration["BackgroundJobs:MessageOutbox:Schedule"]);

        // Cleanup expired refresh tokens
        recurringJobManager.AddOrUpdate<IRefreshTokenCleanup>(
            "refresh-token-cleanup",
            job => job.CleanupExpiredTokensAsync(),
            app.Configuration["BackgroundJobs:RefreshTokenCleanup:Schedule"]);

        // Cleanup expired refresh tokens
        recurringJobManager.AddOrUpdate<NotificationProcessJob>(
            "notification-process",
            job => job.ProcessNotificationsAsync(),
            app.Configuration["BackgroundJobs:NotificationProcess:Schedule"]);


        return app;
    }
}
