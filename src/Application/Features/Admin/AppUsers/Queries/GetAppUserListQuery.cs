using Application.Common.Abstractions;
using Application.Common.Abstractions.Messaging;
using Application.Common.DapperQueries;
using Application.Common.Security;
using Application.Features.Admin.AppUsers.Models;
using Application.Features.Admin.AppNotifications.Queries;

namespace Application.Features.Admin.AppUsers.Queries;

[Authorize(Policy = Permissions.Admin.ApplicationUsers.View)]

public record GetAppUserListQuery
    : DapperPaginatedData, ICacheableQuery<PaginatedResponse<AppUserModel>>
{
    [JsonIgnore]
    public string CacheKey => $"AppUserModel_{Offset}_{Next}";
    public TimeSpan? Expiration => null;
    public bool? AllowCache => null;
}

internal sealed class GetAppUserListQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetAppUserListQuery, PaginatedResponse<AppUserModel>>
{
    public async Task<Result<PaginatedResponse<AppUserModel>>> Handle(GetAppUserListQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
                SELECT 
                    U.Id AS {nameof(AppUserModel.Id)}, 
                    U.FirstName AS {nameof(AppUserModel.FirstName)}, 
                    U.LastName AS {nameof(AppUserModel.LastName)}, 
                    U.Username AS {nameof(AppUserModel.Username)}, 
                    U.Email AS {nameof(AppUserModel.Email)} , 
                    U.PhoneNumber AS {nameof(AppUserModel.PhoneNumber)},
                    IIF(U.IsActive = 1, 'Active', 'Inactive') AS {nameof(AppUserModel.Status)},
                    U.PhotoUrl AS {nameof(AppUserModel.PhotoUrl)},
                    STRING_AGG(R.Name, ', ') AS {nameof(AppUserModel.AssignedRoles)}
                FROM [identity].Users AS U
                LEFT JOIN [identity].UserRoles AS UR ON UR.UserId = U.Id
                LEFT JOIN [identity].Roles AS R ON R.Id = UR.RoleId
                GROUP BY 
                    U.Id, U.FirstName, U.LastName, U.Username, 
                    U.Email, U.PhoneNumber, U.IsActive, U.PhotoUrl
                """;


        return await PaginatedResponse<AppUserModel>
                   .CreateAsync(
                       connection,
                       sql,
                       request.Offset,
                       request.Next);

    }
}
