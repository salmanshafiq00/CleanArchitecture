using Application.Common.Abstractions.Identity;
using Application.Features.Admin.AppNotifications.Commands;
using Application.Features.Admin.AppNotifications.Queries;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Admin;

public class AppNotifications : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapPost("GetAll", GetAll)
                .WithName("GetAppNotifications")
                .Produces<PaginatedResponse<AppNotificationModel>>(StatusCodes.Status200OK);

        app.MapGroup(this)
            .MapGet("GetByUser", GetByUser)
                .WithName("GetAppNotification")
                .Produces<List<AppNotificationModel>>(StatusCodes.Status200OK);

        app.MapGroup(this)
           .MapPost("MarkAsSeen", MarkAsSeen)
           .WithName("MarkAsSeen")
           .Produces(StatusCodes.Status200OK);

    }

    public async Task<IResult> GetAll(ISender sender, [FromBody] GetAppNotificationListQuery query)
    {
        var result = await sender.Send(query)
            .ConfigureAwait(false);

        return TypedResults.Ok(result.Value);
    }

    public async Task<IResult> GetByUser(ISender sender, IUser user, int page, int size)
    {
        var result = await sender.Send(new GetAppNotificationsByUserIdQuery(user.Id, page, size))
            .ConfigureAwait(false);

        return TypedResults.Ok(result.Value);
    }

    public async Task<IResult> MarkAsSeen(ISender sender)
    {
        await sender.Send(new MarkSeenUserNotificationCommand())
            .ConfigureAwait(false);

        return TypedResults.Ok();
    }

}
