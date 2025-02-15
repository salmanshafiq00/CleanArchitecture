namespace Application.Common.Abstractions.Caching;

public interface IDistributedCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellation = default);

    Task<string?> GetStringAsync(string key, CancellationToken cancellation = default);

    Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken cancellation = default);

    Task SetStringAsync(string key, string value, TimeSpan? slidingExpiration = null, CancellationToken cancellation = default);

    Task RemoveAsync(string key, CancellationToken cancellation = default);

    Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellation = default);

    void Refresh(string key);

    Task RefreshAsync(string key, CancellationToken cancellationToken = default);

    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? slidingExpiration = null, CancellationToken cancellation = default);

}
