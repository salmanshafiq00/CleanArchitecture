using Application.Common.Abstractions.Caching;
using Application.Common.Events;

namespace Application.Common.EventHandlers;

internal class CacheInvalidationEventHandler(IDistributedCacheService distributedCache, ILogger<CacheInvalidationEventHandler> logger)
    : INotificationHandler<CacheInvalidationEvent>
{
    public async Task Handle(CacheInvalidationEvent notification, CancellationToken cancellationToken)
    {
        //await distributedCache.RemoveByPrefixAsync(notification.CacheKey, cancellationToken);

        if (notification.CacheKeys is not null && notification.CacheKeys?.Length > 0)
        {
            var tasks = notification.CacheKeys.Select(async cacheKey =>
            {
                await distributedCache.RemoveByPrefixAsync(cacheKey, cancellationToken);
                logger.LogInformation("Cache value of {CacheKey} expired with {@Request}", cacheKey, notification);
            });

            await Task.WhenAll(tasks);
        }
    }
}
