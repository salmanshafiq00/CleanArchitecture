using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;
using Application.Common.DapperQueries;
using Application.Common.Security;

namespace Application.Features.Setups.LookupDetails.Queries;

[Authorize(Policy = Permissions.CommonSetup.LookupDetails.View)]
public record GetLookupDetailListQuery
    : DapperPaginatedData, ICacheableQuery<PaginatedResponse<LookupDetailModel>>
{
    [JsonInclude]
    public string CacheKey => $"{AppCacheKeys.LookupDetail}:{Offset}:{Next}";

    public TimeSpan? Expiration => null;

    public bool? AllowCache => null;
}

internal sealed class GetLookupDetailListQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetLookupDetailListQuery, PaginatedResponse<LookupDetailModel>>
{
    public async Task<Result<PaginatedResponse<LookupDetailModel>>> Handle(GetLookupDetailListQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
            SELECT 
                ld.Id  AS {nameof(LookupDetailModel.Id)}, 
                ld.Name AS {nameof(LookupDetailModel.Name)}, 
                ld.Code AS {nameof(LookupDetailModel.Code)}, 
                ld.ParentId AS {nameof(LookupDetailModel.ParentId)}, 
                p.Name AS {nameof(LookupDetailModel.ParentName)}, 
                ld.Description AS {nameof(LookupDetailModel.Description)},
                IIF(ld.Status = 1, 'Active', 'Inactive') AS {nameof(LookupDetailModel.StatusName)},
                ld.LookupId AS {nameof(LookupDetailModel.LookupId)}, 
                l.Name AS {nameof(LookupDetailModel.LookupName)}
            FROM dbo.LookupDetails AS ld
            INNER JOIN dbo.Lookups l ON l.Id = ld.LookupId
            LEFT JOIN dbo.LookupDetails AS p ON p.Id = ld.ParentId
            """;

        return await PaginatedResponse<LookupDetailModel>
           .CreateAsync(
               connection,
               sql,
               request.Offset,
               request.Next,
               orderBy: "l.Name, ld.Name");
    }
}
