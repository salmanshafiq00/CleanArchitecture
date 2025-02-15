using Application.Common.Abstractions;
using Infrastructure.BackgroundJobs;
using Infrastructure.Caching;
using Infrastructure.Communications;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    private const string DefaultConnection = nameof(DefaultConnection);
    private const string RedisCache = nameof(RedisCache);

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddPersistenceServices(configuration);
        services.AddCachingService(configuration);
        services.AddIdentityService(configuration);
        AddScopedServices(services);
        services.AddHangfireService(configuration);
        services.AddEmailServices(configuration);
        services.AddNotificationServices();
        services.AddSingleton(TimeProvider.System);
        AddHealthChecks(services, configuration);

        return services;
    }

    private static void AddScopedServices(IServiceCollection services)
    {
        services.AddScoped<IDateTimeProvider, DateTimeService>();
        services.AddScoped<IFileService, FileService>();
    }

    private static void AddHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        var dbConString = configuration.GetConnectionString(DefaultConnection);
        var redisConString = configuration.GetConnectionString(RedisCache);

        Guard.Against.Null(dbConString, message: $"Connection string '{nameof(DefaultConnection)}' not found.");
        Guard.Against.Null(redisConString, message: "Connection string 'RedisCache' not found.");

        services.AddHealthChecks()
            .AddSqlServer(dbConString, name: "SQL Server")
            .AddRedis(redisConString, name: "Redis");
    }
}
