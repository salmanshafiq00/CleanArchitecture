namespace Application.Common.Abstractions.Caching;

//public interface IInMemoryCacheService
//{
//    Task<T> GetOrCreateAsync<T>(
//        string key,
//        Func<CancellationToken, Task<T>> factory,
//        TimeSpan? expiration = null,
//        CancellationToken cancellationToken = default);

//    Task Remove(string key);

//}

public interface IInMemoryCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellation = default);
    Task<string?> GetStringAsync(string key, CancellationToken cancellation = default);
    Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken cancellation = default);
    Task SetStringAsync(string key, string value, TimeSpan? slidingExpiration = null, CancellationToken cancellation = default);
    Task RemoveAsync(string key, CancellationToken cancellation = default);
    Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellation = default);
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? slidingExpiration = null, CancellationToken cancellation = default);
}
