using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;

namespace Application.Common.Behaviours;

internal sealed class InMemoryCacheInvalidationBehaviour<TRequest, TResponse>(
    ILogger<InMemoryCacheInvalidationBehaviour<TRequest, TResponse>> logger,
    IInMemoryCacheService cacheService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCacheInvalidatorCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (request.CacheKeys is not null && request.CacheKeys.Length > 0)
        {
            var tasks = request.CacheKeys.Select(async cacheKey =>
            {
                await cacheService.RemoveByPrefixAsync(cacheKey, cancellationToken);
                logger.LogInformation(
                    "Memory cache value of {CacheKey} invalidated with {@Request}",
                    cacheKey,
                    request);
            });

            await Task.WhenAll(tasks);
        }

        return response;
    }
}
