using Application.Common.Abstractions;
using Application.Common.Abstractions.Messaging;
using Application.Common.Models;

namespace Application.Features.Common.Queries;

public record GetSelectListQuery(
    string Sql,
    object Parameters,
    string Key,
    bool? AllowCacheList = null)
    : ICacheableQuery<List<SelectListModel>>
{
    public TimeSpan? Expiration => null;
    public string CacheKey => Key;
    public bool? AllowCache => AllowCacheList ?? true;
}

internal sealed class GetSelectListQueryHandler(
    ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetSelectListQuery, List<SelectListModel>>
{
    public async Task<Result<List<SelectListModel>>> Handle(GetSelectListQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var selectList = await connection
            .QueryAsync<SelectListModel>(request.Sql, request.Parameters);

        return Result.Success(selectList.AsList());
    }
}
