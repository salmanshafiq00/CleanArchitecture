using Application.Common.Abstractions.Communication;
using Application.Common.Abstractions.Identity;
using Application.Common.Constants;
using Application.Features.Admin.AppUsers.Commands;
using Application.Features.Admin.AppUsers.Models;
using Application.Features.Admin.AppUsers.Queries;
using Application.Features.Common.Queries;
using Infrastructure.Communications;
using Microsoft.AspNetCore.SignalR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Identity;

public class Users : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost("GetAll", GetAll)
            .WithName("GetUsers")
            .Produces<PaginatedResponse<AppUserModel>>(StatusCodes.Status200OK);

        group.MapGet("Get/{id}", Get)
            .WithName("GetUser")
            .Produces<AppUserModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("GetProfile", GetProfile)
            .WithName("GetProfile")
            .Produces<AppUserModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("Create", Create)
            .WithName("CreateUser")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("Update", Update)
            .WithName("UpdateUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("Delete/{id}", Delete)
            .WithName("DeleteAppUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);


        group.MapPut("ChangePhoto", ChangePhoto)
            .WithName("ChangePhoto")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("UpdateBasic", UpdateBasic)
            .WithName("UpdateBasic")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("AddToRoles", AddToRoles)
            .WithName("AddToRoles")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private async Task<IResult> GetAll(ISender sender, [FromBody] GetAppUserListQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result.Value);
    }

    private async Task<IResult> Get(ISender sender, [FromRoute] string id)
    {
        var result = await sender.Send(new GetAppUserByIdQuery(id));

        var roleSelectList = await sender.Send(new GetSelectListQuery(
                Sql: SelectListSqls.GetRoleByNameSelectListSql,
                Parameters: new { },
                Key: AppCacheKeys.Role_All_SelectList,
                AllowCacheList: false)
            );
        result.Value.OptionsDataSources.Add("roleSelectList", roleSelectList.Value);

        return result.Match(
            onSuccess: () => Results.Ok(result.Value),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> GetProfile(ISender sender, IUser user)
    {
        if (user is null) return Results.NotFound(ErrorMessages.USER_NOT_FOUND);

        var result = await sender.Send(new GetAppUserProfileQuery(user.Id!));

        return result.Match(
            onSuccess: () => Results.Ok(result.Value),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> Create(ISender sender, [FromBody] CreateAppUserCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
            onSuccess: () => Results.Ok(result.Value),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> Update(
        ISender sender,
        IHubContext<NotificationHub, INotificationHub> signalrContext,
        [FromBody] UpdateAppUserCommand command)
    {
        var result = await sender.Send(command);

        if (result.IsSuccess)
        {
            await signalrContext.Clients.All.ReceiveRolePermissionNotify();
        }

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: result.ToProblemDetails);
    }

    public async Task<IResult> Delete(ISender sender, [FromRoute] string id)
    {
        var result = await sender.Send(new DeleteAppUserCommand(id));
        return result.Match(
             onSuccess: () => Results.NoContent(),
             onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> ChangePhoto(ISender sender, [FromBody] ChangeUserPhotoCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> UpdateBasic(
        ISender sender,
        [FromBody] UpdateAppUserBasicCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> AddToRoles(ISender sender, [FromBody] AddToRolesCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: result.ToProblemDetails);
    }
}
