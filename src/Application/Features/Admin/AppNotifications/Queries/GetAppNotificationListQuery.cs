using Application.Common.Abstractions;
using Application.Common.Abstractions.Messaging;
using Application.Common.DapperQueries;

namespace Application.Features.Admin.AppNotifications.Queries;

public record GetAppNotificationListQuery
    : DapperPaginatedData, ICacheableQuery<PaginatedResponse<AppNotificationModel>>
{
    [JsonIgnore]
    public string CacheKey => $"AppNotification_{Offset}_{Next}";
    public TimeSpan? Expiration => null;
    public bool? AllowCache => null;
}


internal sealed class GetAppNotificationListQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetAppNotificationListQuery, PaginatedResponse<AppNotificationModel>>
{
    public async Task<Result<PaginatedResponse<AppNotificationModel>>> Handle(GetAppNotificationListQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
            SELECT 
                t.Id AS {nameof(AppNotificationModel.Id)}, 
                t.SenderId AS {nameof(AppNotificationModel.SenderId)}, 
                t.RecieverId AS {nameof(AppNotificationModel.RecieverId)}, 
                t.Title AS {nameof(AppNotificationModel.Title)},
                t.Description AS {nameof(AppNotificationModel.Description)},
                t.Url AS {nameof(AppNotificationModel.Url)},
                t.Created AS {nameof(AppNotificationModel.Created)}
            FROM [dbo].AppNotifications AS t
            """;

        return await PaginatedResponse<AppNotificationModel>
                   .CreateAsync(
                       connection,
                       sql,
                       request.Offset,
                       request.Next);

    }
}
