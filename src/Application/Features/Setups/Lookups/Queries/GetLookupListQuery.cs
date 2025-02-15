using Application.Common.Abstractions;
using Application.Common.Abstractions.Messaging;
using Application.Common.DapperQueries;
using static Application.Common.DapperQueries.SqlConstants;
using static Application.Common.Security.Permissions;

namespace Application.Features.Setups.Lookups.Queries;

[Authorize(Policy = CommonSetup.Lookups.View)]
public record GetLookupListQuery
    : DapperPaginatedData, ICacheableQuery<PaginatedResponse<LookupModel>>
{
    [JsonIgnore]
    public string CacheKey => $"Lookup:{Offset}:{Next}";

    public TimeSpan? Expiration => null;

    public bool? AllowCache => null;
}

internal sealed class GetLookupListQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetLookupListQuery, PaginatedResponse<LookupModel>>
{
    public async Task<Result<PaginatedResponse<LookupModel>>> Handle(GetLookupListQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
            SELECT 
                l.Id AS {nameof(LookupModel.Id)}, 
                l.Name AS {nameof(LookupModel.Name)}, 
                l.Code AS {nameof(LookupModel.Code)}, 
                l.ParentId AS {nameof(LookupModel.ParentId)}, 
                p.Name AS {nameof(LookupModel.ParentName)} , 
                l.Description AS {nameof(LookupModel.Description)},
                IIF(l.Status = 1, 'Active', 'Inactive') AS {nameof(LookupModel.StatusName)},
                {S.CONV}(DATE, l.Created) AS {nameof(LookupModel.Created)}
            FROM dbo.Lookups AS l
            LEFT JOIN dbo.Lookups AS p ON p.Id = l.ParentId
            """;

        return await PaginatedResponse<LookupModel>
                   .CreateAsync(
                       connection,
                       sql,
                       request.Offset,
                       request.Next,
                       orderBy: "l.Name");
    }
}
