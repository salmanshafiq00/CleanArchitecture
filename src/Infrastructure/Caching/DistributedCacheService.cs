using System.Collections.Concurrent;
using System.Text.Json;
using Application.Common.Abstractions.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Caching;

internal sealed class DistributedCacheService(
    IDistributedCache distributedCache,
    IOptions<CacheOptions> cacheOptions,
    ConnectionMultiplexer connectionMultiplexer)
    : IDistributedCacheService
{
    private readonly CacheOptions _cacheOptions = cacheOptions.Value;
    private static readonly ConcurrentDictionary<string, bool> CacheKeys = new();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellation = default)
    {
        string? cachedValue = await distributedCache
            .GetStringAsync(key, cancellation);

        return cachedValue is null
            ? default
            : JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task<string?> GetStringAsync(string key, CancellationToken cancellation = default)
    {
        return await distributedCache
            .GetStringAsync(key, cancellation);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellation = default)
    {
        await distributedCache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            GetOptions(slidingExpiration),
            cancellation);

        CacheKeys.TryAdd(key, false);
    }

    public async Task SetStringAsync(
        string key,
        string cacheValue,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellation = default)
    {
        await distributedCache.SetStringAsync(key, cacheValue, GetOptions(slidingExpiration), cancellation);

        CacheKeys.TryAdd(key, false);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellation = default)
    {
        await distributedCache.RemoveAsync(key, cancellation);

        CacheKeys.TryRemove(key, out bool _);
    }

    //public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellation = default)
    //{
    //    var keys = CacheKeys
    //        .Keys
    //        .Where(k => string.Equals(prefixKey, PrefixValue(k)));

    //    var tasks = CacheKeys
    //        .Keys
    //        .Where(k => string.Equals(prefixKey, PrefixValue(k)))
    //        .Select(k => RemoveAsync(k, cancellation));

    //    await Task.WhenAll(tasks);
    //}

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellation = default)
    {
        var keys2 = CacheKeys // Test purpose
            .Keys
            .Where(k => string.Equals(prefixKey, PrefixValue(k)));

        if (string.IsNullOrEmpty(prefixKey))
            return;

        // Check if the cache keys exist in the concurrent dictionary
        var keys = CacheKeys.Keys.Where(k => string.Equals(prefixKey, PrefixValue(k)));

        // If keys are not found in the concurrent dictionary, fetch them from Redis
        if (!keys.Any())
        {
            // Fetch the keys from Redis using the specified prefix pattern
            keys = GetKeysFromRedis(prefixKey);
        }

        // Remove the keys asynchronously
        var tasks = keys.Select(key => RemoveAsync(key, cancellation));

        await Task.WhenAll(tasks);
    }

    public void Refresh(string key)
    {
        distributedCache.Refresh(key);
    }

    public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        await distributedCache.RefreshAsync(key, cancellationToken);
    }


    public async Task<T?> GetOrSetAsync<T>(string key,
        Func<Task<T>> factory,
        TimeSpan? slidingExpiration = null,
        CancellationToken cancellation = default)
    {
        T? cachedValue = await GetAsync<T>(key, cancellation);

        if (cachedValue is not null)
            return cachedValue;

        cachedValue = await factory();

        await SetAsync(key, cachedValue, slidingExpiration, cancellation);

        return cachedValue;
    }

    private DistributedCacheEntryOptions GetOptions(TimeSpan? slidingExpiration)
    {
        var options = new DistributedCacheEntryOptions();
        return slidingExpiration.HasValue
            ? options.SetSlidingExpiration(slidingExpiration.Value)
            : options.SetSlidingExpiration(TimeSpan.FromMinutes(_cacheOptions.SlidingExpiration));
    }

    private static string PrefixValue(string input, char delimiter = ':')
    {
        string[] parts = input.Split(delimiter);
        return parts.Length > 0 ? parts[0] : input;
    }

    private IEnumerable<string> GetKeysFromRedis(string prefixKey)
    {
        var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());

        // Get exact matches and prefix matches
        var exactMatches = server.Keys(pattern: prefixKey);
        var prefixMatches = server.Keys(pattern: $"{prefixKey}:*");

        return exactMatches.Concat(prefixMatches).Select(k => k.ToString());
    }

}
