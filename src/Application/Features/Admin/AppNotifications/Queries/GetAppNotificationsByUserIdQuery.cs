namespace Application.Features.Admin.AppNotifications.Queries;

public record GetAppNotificationsByUserIdQuery(string UserId, int Page, int Size)
    : ICacheableQuery<List<AppNotificationModel>>
{
    [JsonIgnore]
    public string CacheKey => $"AppNotification_{UserId}_Page_{Page}_Size_{Size}";

    public bool? AllowCache => false;

    public TimeSpan? Expiration => null;
}


internal sealed class GetAppNotificationsByUserIdQueryHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetAppNotificationsByUserIdQuery, List<AppNotificationModel>>
{
    public async Task<Result<List<AppNotificationModel>>> Handle(GetAppNotificationsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
            SELECT TOP 20
                n.Id AS {nameof(AppNotificationModel.Id)}, 
                n.SenderId AS {nameof(AppNotificationModel.SenderId)}, 
                n.RecieverId AS {nameof(AppNotificationModel.RecieverId)}, 
                n.Title AS {nameof(AppNotificationModel.Title)},
                n.Description AS {nameof(AppNotificationModel.Description)},
                n.Url AS {nameof(AppNotificationModel.Url)},
                n.IsSeen AS {nameof(AppNotificationModel.IsSeen)},
                n.Created AS {nameof(AppNotificationModel.Created)}
            FROM [dbo].AppNotifications AS n
            WHERE n.RecieverId = @UserId
            ORDER BY n.Created DESC
            --OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY
            """;

        var result = await connection.QueryAsync<AppNotificationModel>(sql, new
        {
            request.UserId,
            Offset = request.Page * request.Size,
            request.Size
        });

        return result.AsList();
    }
}

