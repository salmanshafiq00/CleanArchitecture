using Domain.Shared;

namespace Application.Common.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand
{
}

public interface ICommand : IRequest<Result>, IBaseCommand
{
}

public interface IBaseCommand
{
}
