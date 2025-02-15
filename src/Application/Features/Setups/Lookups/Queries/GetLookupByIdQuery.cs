using Application.Common.Abstractions;
using Application.Common.Abstractions.Messaging;
using Application.Common.Extensions;
using Application.Common.Security;

namespace Application.Features.Setups.Lookups.Queries;

[Authorize(Policy = Permissions.CommonSetup.Lookups.View)]
public record GetLookupByIdQuery(Guid? Id) : ICacheableQuery<LookupModel>
{
    [JsonIgnore]
    public string CacheKey => $"Lookup:{Id}";
    [JsonIgnore]
    public TimeSpan? Expiration => null;
    public bool? AllowCache => false;
}

internal sealed class GetLookupByIdQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetLookupByIdQuery, LookupModel>
{
    public async Task<Result<LookupModel>> Handle(GetLookupByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.Id.IsNullOrEmpty())
        {
            return new LookupModel() { Status = true };
        }
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
            SELECT 
                L.Id AS {nameof(LookupModel.Id)}, 
                L.Name AS {nameof(LookupModel.Name)}, 
                L.Code AS {nameof(LookupModel.Code)}, 
                L.ParentId AS {nameof(LookupModel.ParentId)}, 
                L.Description AS {nameof(LookupModel.Description)},
                L.Status AS {nameof(LookupModel.Status)}
            FROM dbo.Lookups AS l
            LEFT JOIN dbo.Lookups AS p ON p.Id = l.ParentId
            WHERE l.Id = @Id
            """;

        return await connection.QueryFirstOrDefaultAsync<LookupModel>(sql, new { query.Id });
    }
}
