using Application.Common.Abstractions;
using Application.Common.Abstractions.Messaging;
using Application.Common.Extensions;

namespace Application.Features.Setups.LookupDetails.Queries;

//[Authorize(Policy = Permissions.CommonSetup.LookupDetails.View)]
public record GetLookupDetailByIdQuery(Guid Id) : ICacheableQuery<LookupDetailModel>
{
    [JsonIgnore]
    public string CacheKey => $"LookupDetail_{Id}";
    [JsonIgnore]
    public TimeSpan? Expiration => null;
    public bool? AllowCache => false;
}

internal sealed class GetLookupDetailByIdQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetLookupDetailByIdQuery, LookupDetailModel>
{
    public async Task<Result<LookupDetailModel>> Handle(GetLookupDetailByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.Id.IsNullOrEmpty())
        {
            return new LookupDetailModel();
        }
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
            SELECT 
                ld.Id AS {nameof(LookupDetailModel.Id)}, 
                ld.Name AS {nameof(LookupDetailModel.Name)}, 
                ld.Code AS {nameof(LookupDetailModel.Code)}, 
                ld.ParentId  AS {nameof(LookupDetailModel.ParentId)}, 
                ld.Description AS {nameof(LookupDetailModel.Description)},
                ld.Status AS {nameof(LookupDetailModel.Status)},
                ld.LookupId AS {nameof(LookupDetailModel.LookupId)}
            FROM dbo.LookupDetails AS ld
            WHERE ld.Id = @Id
            """;

        return await connection.QueryFirstOrDefaultAsync<LookupDetailModel>(sql, new { query.Id });
    }
}
