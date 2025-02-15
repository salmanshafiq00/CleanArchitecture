using Application.Common.Abstractions.Communication;
using Application.Features.Admin.AppMenus.Queries;
using Application.Features.Admin.Roles.Commands;
using Application.Features.Admin.Roles.Models;
using Application.Features.Admin.Roles.Queries;
using Infrastructure.Communications;
using Microsoft.AspNetCore.SignalR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Identity;

public class Roles : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost("GetAll", GetAll)
            .WithName("GetRoles")
            .Produces<PaginatedResponse<RoleModel>>(StatusCodes.Status200OK);

        group.MapGet("Get/{id}", Get)
            .WithName("GetRole")
            .Produces<RoleModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("Create", Create)
            .WithName("CreateRole")
            .Produces<string>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("Update", Update)
            .WithName("UpdateRole")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("Delete/{id}", Delete)
            .WithName("DeleteRole")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);


        group.MapGet("GetRolePermissions/{roleId}", GetRolePermissions)
            .WithName("GetRolePermissions")
            .Produces<RolePermissionModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("AddOrRemovePermissions", AddOrRemovePermissions)
            .WithName("AddOrRemovePermissions")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);


        group.MapGet("GetRoleMenus/{roleId}", GetRoleMenus)
            .WithName("GetRoleMenus")
            .Produces<RoleMenuModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("AddOrRemoveMenus", AddOrRemoveMenus)
            .WithName("AddOrRemoveMenus")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

    }

    private async Task<IResult> GetAll(ISender sender, [FromBody] GetRoleListQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result.Value);
    }

    private async Task<IResult> Get(ISender sender, [FromRoute] string id)
    {
        var result = await sender.Send(new GetRoleByIdQuery(id));

        //var permissionNodeList = await sender.Send(new GetPermissionTreeSelectListQuery()).ConfigureAwait(false);
        //result.Value.OptionsDataSources["permissionNodeList"] = permissionNodeList.Value;

        //var appMenuTreeList = await sender.Send(new GetAppMenuTreeSelectList()).ConfigureAwait(false);
        //result.Value.OptionsDataSources["appMenuTreeList"] = appMenuTreeList.Value;

        return result.Match(
            onSuccess: () => Results.Ok(result.Value),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> Create(ISender sender, [FromBody] CreateRoleCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
            onSuccess: () => Results.Ok(result.Value),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> Update(
        ISender sender,
        IHubContext<NotificationHub, INotificationHub> signalrContext,
        [FromBody] UpdateRoleCommand command)
    {
        var result = await sender.Send(command);

        //if (result.IsSuccess)
        //{
        //    await signalrContext.Clients.All.ReceiveRolePermissionNotify();
        //}

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: result.ToProblemDetails);
    }

    public async Task<IResult> Delete(ISender sender, [FromRoute] string id)
    {
        var result = await sender.Send(new DeleteRoleCommand(id));
        return result.Match(
             onSuccess: () => Results.NoContent(),
             onFailure: result.ToProblemDetails);
    }


    private async Task<IResult> GetRolePermissions(ISender sender, [FromRoute] string roleId)
    {
        var result = await sender.Send(new GetPermissionsByRoleQuery(roleId));

        var permissionNodeList = await sender.Send(new GetPermissionTreeSelectListQuery()).ConfigureAwait(false);
        result.Value.OptionsDataSources["permissionNodeList"] = permissionNodeList.Value;

        return result.Match(
            onSuccess: () => Results.Ok(result.Value),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> AddOrRemovePermissions(
        ISender sender,
        IHubContext<NotificationHub, INotificationHub> signalrContext,
        [FromBody] AddOrRemovePermissionCommand command)
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

    private async Task<IResult> GetRoleMenus(ISender sender, [FromRoute] string roleId)
    {
        var result = await sender.Send(new GetMenusByRoleQuery(roleId));

        var appMenuTreeList = await sender.Send(new GetAppMenuTreeSelectList()).ConfigureAwait(false);
        result.Value.OptionsDataSources["appMenuTreeList"] = appMenuTreeList.Value;

        return result.Match(
            onSuccess: () => Results.Ok(result.Value),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> AddOrRemoveMenus(
        ISender sender,
        IHubContext<NotificationHub, INotificationHub> signalrContext,
        [FromBody] AddOrRemoveMenusCommand command)
    {
        var result = await sender.Send(command);

        if (result.IsSuccess)
        {
            await signalrContext.Clients.All.ReceiveRoleMenuNotify();
        }

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: result.ToProblemDetails);
    }

}
