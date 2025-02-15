namespace Application.Common.Abstractions.Messaging;

public interface ICacheInvalidatorCommand<TResponse> : ICommand<TResponse>, IBaseCacheInvalidatorCommand
{
}

public interface ICacheInvalidatorCommand : ICommand, IBaseCacheInvalidatorCommand
{
}

public interface IBaseCacheInvalidatorCommand
{
    string[] CacheKeys { get; }
}
