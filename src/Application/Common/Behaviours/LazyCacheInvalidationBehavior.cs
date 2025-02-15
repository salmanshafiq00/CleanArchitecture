using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;

namespace Application.Common.Behaviours;

internal sealed class LazyCacheInvalidationBehavior<TRequest, TResponse>(
    ILazyCacheService cacheService,
    ILogger<LazyCacheInvalidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCacheInvalidatorCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next().ConfigureAwait(false);

        if (request.CacheKeys?.Length > 0)
        {
            var tasks = request.CacheKeys.Select(async cacheKey =>
            {
                await cacheService.RemoveByPrefixAsync(cacheKey, cancellationToken);
                logger.LogInformation("Cache invalidated for key pattern: {CacheKey} by {RequestName}",
                    cacheKey, typeof(TRequest).Name);
            });

            await Task.WhenAll(tasks);
        }

        return response;
    }
}
