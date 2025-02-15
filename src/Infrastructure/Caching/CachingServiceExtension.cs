using Application.Common.Abstractions.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure.Caching;

internal static class CachingServiceExtension
{
    private const string RedisCache = nameof(RedisCache);

    public static IServiceCollection AddCachingService(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureOptions<CacheOptionsSetup>();

        #region Distrubuted Cache
        var redisConString = configuration.GetConnectionString(RedisCache);
        Guard.Against.Null(redisConString, message: "Connection string 'RedisCache' not found.");
        services.AddSingleton(ConnectionMultiplexer.Connect(redisConString));
        services.AddStackExchangeRedisCache(options => options.Configuration = redisConString);
        services.AddDistributedMemoryCache();
        services.AddSingleton<IDistributedCacheService, DistributedCacheService>();

        #endregion

        #region In-Memory Cache
        services.AddMemoryCache();
        services.AddSingleton<IInMemoryCacheService, InMemoryCacheService>();

        #endregion

        #region Lazy Cache

        services.AddLazyCache();
        services.AddSingleton<ILazyCacheService, LazyCacheService>();

        #endregion

        return services;
    }
}
