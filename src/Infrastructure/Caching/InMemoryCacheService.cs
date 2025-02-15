using System.Collections.Concurrent;
using Application.Common.Abstractions.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Infrastructure.Caching;

internal sealed class InMemoryCacheService(
    IMemoryCache memoryCache,
    IOptions<CacheOptions> cacheOptions,
    ILogger<InMemoryCacheService> logger)
    : IInMemoryCacheService
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;
    private static readonly ConcurrentDictionary<string, bool> CacheKeys = new();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellation = default)
    {
        return memoryCache.TryGetValue(key, out T? value) ? value : default;
    }

    public async Task<string?> GetStringAsync(string key, CancellationToken cancellation = default)
    {
        return memoryCache.TryGetValue(key, out string? value) ? value : default;
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellation = default)
    {
        var options = GetOptions(slidingExpiration);
        memoryCache.Set(key, value, options);
        CacheKeys.TryAdd(key, false);
    }

    public async Task SetStringAsync(
        string key,
        string value,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellation = default)
    {
        var options = GetOptions(slidingExpiration);
        memoryCache.Set(key, value, options);
        CacheKeys.TryAdd(key, false);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellation = default)
    {
        memoryCache.Remove(key);
        CacheKeys.TryRemove(key, out _);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellation = default)
    {
        if (string.IsNullOrEmpty(prefixKey))
            return;

        var keys = CacheKeys.Keys
        .Where(k => k.Equals(prefixKey, StringComparison.OrdinalIgnoreCase) ||
                    k.StartsWith($"{prefixKey}:", StringComparison.OrdinalIgnoreCase))
        .ToList();

        foreach (var key in keys)
        {
            await RemoveAsync(key, cancellation);
            logger.LogInformation("Cache value of {CacheKey} removed with prefix {Prefix}", key, prefixKey);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellation = default)
    {
        if (memoryCache.TryGetValue(key, out T? cachedValue))
            return cachedValue;

        cachedValue = await factory();
        await SetAsync(key, cachedValue, slidingExpiration, cancellation);
        return cachedValue;
    }

    private MemoryCacheEntryOptions GetOptions(TimeSpan? slidingExpiration)
    {
        var options = new MemoryCacheEntryOptions();
        return slidingExpiration.HasValue
            ? options.SetSlidingExpiration(slidingExpiration.Value)
            : options.SetSlidingExpiration(TimeSpan.FromMinutes(_cacheOptions.SlidingExpiration));
    }
}
