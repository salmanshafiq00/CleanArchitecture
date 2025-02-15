using Application.Common.Extensions;
using Application.Features.Common.Queries;
using Application.Features.Setups.LookupDetails.Commands;
using Application.Features.Setups.LookupDetails.Queries;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Endpoints.Setups;

public class LookupDetails : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        group.MapPost("GetAll", GetAll)
             .WithName("GetLookupDetails")
             .Produces<PaginatedResponse<LookupDetailModel>>(StatusCodes.Status200OK);

        group.MapGet("Get/{id:Guid}", Get)
             .WithName("GetLookupDetail")
             .Produces<LookupDetailModel>(StatusCodes.Status200OK);

        group.MapPost("Create", Create)
             .WithName("CreateLookupDetail")
             .Produces<Guid>(StatusCodes.Status201Created)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPut("Update", Update)
             .WithName("UpdateLookupDetail")
             .Produces(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("Delete/{id:Guid}", Delete)
             .WithName("DeleteLookupDetail")
             .Produces(StatusCodes.Status204NoContent)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("DeleteMultiple", DeleteMultiple)
             .WithName("DeleteMultiple")
             .Produces(StatusCodes.Status204NoContent)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("Upload", Upload)
             .WithName("LookupDetailUpload")
             .Produces<int>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    private async Task<IResult> GetAll(ISender sender, GetLookupDetailListQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result.Value);
    }

    private async Task<IResult> Get(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetLookupDetailByIdQuery(id));

        var lookupSelectList = await sender.Send(new GetSelectListQuery(
            Sql: SelectListSqls.GetLookupSelectListSql,
            Parameters: new { },
            Key: AppCacheKeys.Lookup_All_SelectList,
            AllowCacheList: false));

        result.Value.OptionsDataSources.Add("lookupSelectList", lookupSelectList.Value);

        var parentSelectList = await sender.Send(new GetSelectListQuery(
            Sql: SelectListSqls.GetLookupDetailSelectListSql,
            Parameters: new { },
            Key: AppCacheKeys.LookupDetail_All_SelectList,
            AllowCacheList: false));

        result.Value.OptionsDataSources.Add("parentSelectList", parentSelectList.Value);

        return TypedResults.Ok(result.Value);
    }

    private async Task<IResult> Create(ISender sender, [FromBody] CreateLookupDetailCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
            onSuccess: () => Results.CreatedAtRoute("GetLookupDetail", new { id = result.Value }),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> Update(ISender sender, [FromBody] UpdateLookupDetailCommand command)
    {
        var result = await sender.Send(command);

        return result.Match(
            onSuccess: () => Results.Ok(),
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> Delete(ISender sender, Guid id)
    {
        var result = await sender.Send(new DeleteLookupDetailCommand(id));

        return result.Match(
            onSuccess: Results.NoContent,
            onFailure: result.ToProblemDetails);
    }

    private async Task<IResult> DeleteMultiple(ISender sender, [FromBody] Guid[] ids)
    {
        Result? result = null;
        foreach (var id in ids)
        {
            result = await sender.Send(new DeleteLookupDetailCommand(id));
        }
        return result!.Match(
            onSuccess: Results.NoContent,
            onFailure: result!.ToProblemDetails);
    }

    private async Task<IResult> Upload(ISender sender, IHttpContextAccessor contextAccessor)
    {
        var file = contextAccessor.HttpContext.Request.Form.Files[0];

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest("No file uploaded.");
        }

        var result = await sender.Send(new CreateLookupDetailFromExcelCommand(file));

        return result!.Match(
            onSuccess: () => Results.Ok(result.Value),
            onFailure: result!.ToProblemDetails);
    }
}
