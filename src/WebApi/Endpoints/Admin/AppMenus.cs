using Application.Common.Abstractions.Communication;
using Application.Common.Extensions;
using Application.Common.Models;
using Application.Features.Admin.AppMenus.Commands;
using Application.Features.Admin.AppMenus.Queries;
using Application.Features.Common.Queries;
using Domain.Shared;
using Infrastructure.Communications;
using Microsoft.AspNetCore.SignalR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Admin;

public class AppMenus : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost("GetAll", GetAll)
            .WithName("GetAppMenus")
            .Produces<PaginatedResponse<AppMenuModel>>(StatusCodes.Status200OK);

        group.MapGet("GetSidebarMenus", GetSidebarMenus)
            .WithName("GetSidebarMenus")
            .Produces<List<SidebarMenuModel>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("Get/{id}", Get)
            .WithName("GetAppMenu")
            .Produces<AppMenuModel>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("Create", Create)
            .WithName("CreateMenu")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("Update", Update)
            .WithName("UpdateMenu")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("Delete/{id}", Delete)
            .WithName("DeleteMenu")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("GetAllMenuAsTree", GetAllMenuAsTree)
            .WithName("GetAllMenuAsTree")
            .Produces<List<TreeNodeModel>>(StatusCodes.Status200OK);

        group.MapPost("ReorderAppMenus", ReorderAppMenus)
            .WithName("ReorderAppMenus")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);


    }

    public async Task<IResult> GetAll(ISender sender, [FromBody] GetAppMenuListQuery query)
    {
        var result = await sender.Send(query);

        return TypedResults.Ok(result.Value);
    }

    public async Task<IResult> GetAllMenuAsTree(ISender sender)
    {
        var result = await sender.Send(new GetAppMenuTreeSelectList()).ConfigureAwait(false);
        return TypedResults.Ok(result.Value);
    }

    public async Task<IResult> Get(ISender sender, [FromRoute] Guid id)
    {
        var result = await sender.Send(new GetAppMenuByIdQuery(id));

        var parentTreeSelectList = await sender.Send(new GetAppMenuTreeSelectList());
        result?.Value?.OptionsDataSources.Add("parentTreeSelectList", parentTreeSelectList.Value);

        var menuTypeSelectList = await sender.Send(new GetSelectListQuery(
        Sql: SelectListSqls.GetLookupDetailSelectListByDevCodeSql,
        Parameters: new { DevCode = LookupDevCode.MenuType },
        Key: AppCacheKeys.LookupDetail_All_SelectList,
        AllowCacheList: false));
        result?.Value?.OptionsDataSources.Add("menuTypeSelectList", menuTypeSelectList.Value);

        return result.Match(
             onSuccess: () => Results.Ok(result.Value),
             onFailure: result.ToProblemDetails);
    }

    public async Task<IResult> Create(ISender sender, [FromBody] CreateAppMenuCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
             onSuccess: () => Results.Ok(result.Value),
             onFailure: result.ToProblemDetails);
    }

    public async Task<IResult> Update(ISender sender, [FromBody] UpdateAppMenuCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
             onSuccess: () => Results.NoContent(),
             onFailure: result.ToProblemDetails);
    }

    public async Task<IResult> Delete(ISender sender, [FromRoute] Guid id)
    {
        var result = await sender.Send(new DeleteAppMenuCommand(id));
        return result.Match(
             onSuccess: () => Results.NoContent(),
             onFailure: result.ToProblemDetails);
    }

    public async Task<IResult> GetSidebarMenus(ISender sender)
    {
        var result = await sender.Send(new GetSidebarMenuQuery());

        return result.Match(
             onSuccess: () => Results.Ok(result.Value),
             onFailure: result.ToProblemDetails);
    }

    public async Task<IResult> ReorderAppMenus(
        ISender sender,
        IHubContext<NotificationHub, INotificationHub> signalrContext,
        UpdateAppMenuOrderCommand command)
    {
        var result = await sender.Send(command);

        if (result.IsSuccess)
        {
            await signalrContext.Clients.All.ReceiveMenuOrderChangeNotify();
        }

        return result.Match(
             onSuccess: () => Results.NoContent(),
             onFailure: result.ToProblemDetails);
    }

}
