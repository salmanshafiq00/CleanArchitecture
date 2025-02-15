using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;
using Application.Common.Security;
using Application.Features.Admin.AppUsers.Models;

namespace Application.Features.Admin.AppUsers.Queries;

[Authorize(Policy = Permissions.Admin.ApplicationUsers.View)]
public record GetAppUserSelectListQuery
    : ICacheableQuery<List<AppUserSelectListModel>>
{
    [JsonIgnore]
    public string CacheKey => $"{AppCacheKeys.AppUser_Select_List}";

    public TimeSpan? Expiration => null;

    public bool? AllowCache => true;
}

internal sealed class GetAppUserSelectListQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetAppUserSelectListQuery, List<AppUserSelectListModel>>
{
    public async Task<Result<List<AppUserSelectListModel>>> Handle(GetAppUserSelectListQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
                SELECT 
                    U.Id AS {nameof(AppUserSelectListModel.Id)}, 
                    U.FirstName AS {nameof(AppUserSelectListModel.FirstName)}, 
                    U.LastName AS {nameof(AppUserSelectListModel.LastName)}, 
                    U.Username AS {nameof(AppUserSelectListModel.Username)}, 
                    CONCAT(U.FirstName, ' ', COALESCE(U.LastName, ''), ' (', U.Username, ')')  AS {nameof(AppUserSelectListModel.DisplayName)} 
                FROM [identity].Users AS U
                Order By U.FirstName
                """;

        var result = await connection.QueryAsync<AppUserSelectListModel>(sql);
        return result.AsList();

    }
}
