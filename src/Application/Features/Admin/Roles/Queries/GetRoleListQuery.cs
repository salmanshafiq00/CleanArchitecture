using Application.Common.Abstractions;
using Application.Common.Abstractions.Messaging;
using Application.Common.DapperQueries;
using Application.Features.Admin.Roles.Models;

namespace Application.Features.Admin.Roles.Queries;

public record GetRoleListQuery
    : DapperPaginatedData, ICacheableQuery<PaginatedResponse<RoleModel>>
{
    [JsonIgnore]
    public string CacheKey => $"Role:{Offset}:{Next}";
    public TimeSpan? Expiration => null;
    public bool? AllowCache => null;
}

internal sealed class GetRoleListQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetRoleListQuery, PaginatedResponse<RoleModel>>
{
    public async Task<Result<PaginatedResponse<RoleModel>>> Handle(GetRoleListQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
            SELECT 
                R.Id AS {nameof(RoleModel.Id)}, 
                R.Name AS {nameof(RoleModel.Name)} 
            FROM [identity].Roles AS R
            """;

        return await PaginatedResponse<RoleModel>
                   .CreateAsync(
                       connection,
                       sql,
                       request.Offset,
                       request.Next);
    }
}
