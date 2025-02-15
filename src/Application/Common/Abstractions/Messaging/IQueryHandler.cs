using Domain.Shared;

namespace Application.Common.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> :
    IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
