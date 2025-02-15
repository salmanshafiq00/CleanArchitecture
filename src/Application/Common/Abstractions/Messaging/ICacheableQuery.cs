namespace Application.Common.Abstractions.Messaging;

public interface ICacheableQuery<TResponse> : ICacheableQuery, IQuery<TResponse>;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; }
    bool? AllowCache { get; }
}


