using Application.Common.Abstractions.Communication;
using Application.Common.Abstractions.Identity;
using Application.Common.Extensions;
using Application.Common.Models;
using Application.Features.Admin.AppNotifications.Queries;
using Application.Features.Common.Queries;
using Application.Features.Setups.Lookups.Commands;
using Application.Features.Setups.Lookups.Queries;
using Infrastructure.Communications;
using Microsoft.AspNetCore.SignalR;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Setups;

public sealed class Lookups : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost("GetAll", GetAll)
             .WithName("GetLookups")
             .Produces<PaginatedResponse<LookupModel>>(StatusCodes.Status200OK);

        group.MapGet("Get/{id:Guid}", Get)
             .WithName("GetLookup")
             .Produces<LookupModel>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("Create", Create)
             .WithName("CreateLookup")
             .Produces<Guid>(StatusCodes.Status201Created)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("Update", Update)
             .WithName("UpdateLookup")
             .Produces(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("Delete/{id:Guid}", Delete)
             .WithName("DeleteLookup")
             .Produces(StatusCodes.Status204NoContent)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private async Task<IResult> GetAll(
        ISender sender,
        IHubContext<NotificationHub, INotificationHub> context,
        IUser user,
        [FromBody] GetLookupListQuery query)
    {
        var result = await sender.Send(query);

        return result.IsFailure ? result.ToProblemDetails() : TypedResults.Ok(result.Value);
    }

    private async Task<IResult> Get(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetLookupByIdQuery(id));

        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        var parentSelectList = await sender.Send(new GetSelectListQuery(
            Sql: SelectListSqls.GetLookupSelectListSql,
            Parameters: new { },
            Key: AppCacheKeys.Lookup_All_SelectList,
            AllowCacheList: false)
        );
        result?.Value?.OptionsDataSources.Add("parentSelectList", parentSelectList.Value);
        return TypedResults.Ok(result.Value);
    }

    private async Task<IResult> Create(ISender sender, [FromBody] CreateLookupCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
            onSuccess: () => Results.CreatedAtRoute("GetLookup", new { id = result.Value }),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> Update(ISender sender, [FromBody] UpdateLookupCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
            onSuccess: () => Results.Ok(),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> Delete(ISender sender, Guid id)
    {
        var result = await sender.Send(new DeleteLookupCommand(id));

        return result.Match(
            onSuccess: Results.NoContent,
            onFailure: result.ToProblemDetails);
    }

}
