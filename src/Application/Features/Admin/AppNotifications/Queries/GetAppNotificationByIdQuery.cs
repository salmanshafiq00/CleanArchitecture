namespace Application.Features.Admin.AppNotifications.Queries;

public record GetAppNotificationByIdQuery(string Id)
    : ICacheableQuery<AppNotificationModel>
{
    [JsonIgnore]
    public string CacheKey => $"AppNotification_{Id}";

    public bool? AllowCache => true;

    public TimeSpan? Expiration => null;
}

internal sealed class GetAppNotificationByIdQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetAppNotificationByIdQuery, AppNotificationModel>
{
    public async Task<Result<AppNotificationModel>> Handle(GetAppNotificationByIdQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
            SELECT 
                n.Id AS {nameof(AppNotificationModel.Id)}, 
                n.SenderId AS {nameof(AppNotificationModel.SenderId)}, 
                n.RecieverId AS {nameof(AppNotificationModel.RecieverId)}, 
                n.Title AS {nameof(AppNotificationModel.Title)},
                n.Description AS {nameof(AppNotificationModel.Description)},
                n.Url AS {nameof(AppNotificationModel.Url)},
                n.Created AS {nameof(AppNotificationModel.Created)}
            FROM [dbo].AppNotifications AS n
            WHERE n.Id = @Id
            """;
        return await connection.QueryFirstOrDefaultAsync(sql, new { request.Id });
    }
}
