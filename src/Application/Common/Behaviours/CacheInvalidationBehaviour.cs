using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;

namespace Application.Common.Behaviours;

internal sealed class CacheInvalidationBehaviour<TRequest, TResponse>(
    ILogger<CacheInvalidationBehaviour<TRequest, TResponse>> logger,
    IDistributedCacheService distributedCache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCacheInvalidatorCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next().ConfigureAwait(false);
        //if (!string.IsNullOrEmpty(request.CacheKey))
        if (request.CacheKeys is not null && request.CacheKeys?.Length > 0)
        {
            var tasks = request.CacheKeys.Select(async cacheKey =>
            {
                await distributedCache.RemoveByPrefixAsync(cacheKey, cancellationToken);
                logger.LogInformation("Cache value of {CacheKey} expired with {@Request}", cacheKey, request);
            });

            await Task.WhenAll(tasks);
        }

        return response;
    }
}
