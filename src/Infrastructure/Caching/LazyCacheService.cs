using System.Collections.Concurrent;
using Application.Common.Abstractions.Caching;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Caching;

internal sealed class LazyCacheService(
    IAppCache cache,
    IOptions<CacheOptions> cacheOptions,
    ILogger<LazyCacheService> logger) : ILazyCacheService
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;
    private static readonly ConcurrentDictionary<string, bool> CacheKeys = new();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return await cache.GetAsync<T>(key);
    }

    public async Task<string?> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        return await cache.GetAsync<string>(key);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        await cache.GetOrAddAsync(
            key,
            () => Task.FromResult(value),
            GetMemoryCacheOptions(slidingExpiration));
        CacheKeys.TryAdd(key, false);
        logger.LogTrace("Cache set for key: {Key}", key);
    }

    public async Task SetStringAsync(
        string key,
        string value,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        await cache.GetOrAddAsync(
            key,
            () => Task.FromResult(value),
            GetMemoryCacheOptions(slidingExpiration));
        CacheKeys.TryAdd(key, false);
        logger.LogTrace("Cache string set for key: {Key}", key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cache.Remove(key);
        CacheKeys.TryRemove(key, out _);
        logger.LogTrace("Cache removed for key: {Key}", key);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(prefixKey))
            return;

        var keys = CacheKeys.Keys
        .Where(k => k.Equals(prefixKey, StringComparison.OrdinalIgnoreCase) ||
                    k.StartsWith($"{prefixKey}:", StringComparison.OrdinalIgnoreCase))
        .ToList();

        foreach (var key in keys)
        {
            cache.Remove(key);
            CacheKeys.TryRemove(key, out _);
            logger.LogTrace("Cache removed for key: {Key} with prefix: {Prefix}", key, prefixKey);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrAddAsync(
            key,
            async () =>
            {
                var result = await factory();
                CacheKeys.TryAdd(key, false);
                return result;
            },
            GetMemoryCacheOptions(slidingExpiration));
    }

    private MemoryCacheEntryOptions GetMemoryCacheOptions(TimeSpan? slidingExpiration)
    {
        var options = new MemoryCacheEntryOptions();

        if (slidingExpiration.HasValue)
        {
            options.SetSlidingExpiration(slidingExpiration.Value);
        }
        else
        {
            options.SetSlidingExpiration(TimeSpan.FromMinutes(_cacheOptions.SlidingExpiration))
                  .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheOptions.AbsoluteExpiration));
        }

        return options;
    }
}
