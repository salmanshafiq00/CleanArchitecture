using System.Text.Json;

namespace Application.Common.Behaviours;

internal sealed class LazyCachingBehavior<TRequest, TResponse>(
    ILazyCacheService cacheService,
    ILogger<LazyCachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
    where TResponse : Result
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        //ReferenceHandler = ReferenceHandler.Preserve,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IncludeFields = true
    };

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request.AllowCache ?? true)
        {
            string? cachedJson = await cacheService.GetStringAsync(request.CacheKey, cancellationToken);
            if (cachedJson is not null)
            {
                logger.LogInformation("Cache hit for {RequestName} with key {CacheKey}",
                    typeof(TRequest).FullName, request.CacheKey);
                return JsonSerializer.Deserialize<TResponse>(cachedJson, SerializerOptions)!;
            }
        }

        TResponse result = await next();

        if (result.IsSuccess)
        {
            string json = JsonSerializer.Serialize(result, SerializerOptions);

            await cacheService.SetStringAsync(
                request.CacheKey,
                json,
                request.Expiration,
                cancellationToken).ConfigureAwait(false);

            logger.LogInformation("Added to cache: {RequestName} with key {CacheKey}",
                typeof(TRequest).FullName, request.CacheKey);
        }

        return result;
    }
}
