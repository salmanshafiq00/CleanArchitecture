using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;

namespace Application.Common.Behaviours;

internal sealed class InMemoryCachingBehaviour<TRequest, TResponse>(
    IInMemoryCacheService cacheService,
    ILogger<InMemoryCachingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request.AllowCache ?? true)
        {
            TResponse? cachedResult = await cacheService.GetAsync<TResponse>(
                request.CacheKey,
                cancellationToken);

            if (cachedResult is not null)
            {
                logger.LogInformation("Memory cache hit for {RequestName} with key {CacheKey}", typeof(TRequest).FullName, request.CacheKey);
                return cachedResult;
            }
        }

        TResponse result = await next();

        if (result.IsSuccess)
        {
            await cacheService.SetAsync(
                request.CacheKey,
                result,
                request.Expiration,
                cancellationToken);
        }

        return result;
    }
}
