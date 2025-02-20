using System.Data;
using Domain.Admin;

namespace Application.Features.Admin.AppMenus.Queries;

public record GetAppMenuTreeSelectList
    : ICacheableQuery<List<TreeNodeModel>>
{
    [JsonIgnore]
    public string CacheKey => AppCacheKeys.AppMenu_Tree_SelectList;

    public bool? AllowCache => true;

    public TimeSpan? Expiration => null;
}

internal sealed class GetAppMenuTreeSelectListHandler(ISqlConnectionFactory sqlConnection)
    : IQueryHandler<GetAppMenuTreeSelectList, List<TreeNodeModel>>
{
    public async Task<Result<List<TreeNodeModel>>> Handle(GetAppMenuTreeSelectList request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        var sql = $"""
            SELECT 
                M.Id AS {nameof(AppMenuModel.Id)}, 
                M.Label AS {nameof(AppMenuModel.Label)}, 
                M.RouterLink AS {nameof(AppMenuModel.RouterLink)}, 
                M.ParentId AS {nameof(AppMenuModel.ParentId)}, 
                M.Description AS {nameof(AppMenuModel.Description)},
                M.Visible AS {nameof(AppMenuModel.Visible)},
                M.IsActive AS {nameof(AppMenuModel.IsActive)},
                M.Icon AS {nameof(AppMenuModel.Icon)},
                M.OrderNo AS {nameof(AppMenuModel.OrderNo)}
            FROM dbo.AppMenus AS M
            ORDER BY M.OrderNo
            """;

        var appMenus = await connection.QueryAsync<AppMenu>(sql);

        var lookup = appMenus.ToLookup(x => x.ParentId);

        IList<TreeNodeModel> BuildTree(Guid? parentId)
        {
            return lookup[parentId]
                .Select(x => new TreeNodeModel
                {
                    Key = x.Id,
                    Label = x.Label,
                    Icon = x.Icon,
                    ParentId = x.ParentId,
                    Data = x.Description,
                    DisabledCheckbox = false,
                    Visible = x.Visible,
                    IsActive = x.IsActive,
                    PartialSelected = false,
                    OrderNo = x.OrderNo,
                    Children = BuildTree(x.Id)
                })
                .OrderBy(x => x.OrderNo)
                .ToList();
        }

        var tree = BuildTree(null);

        return Result.Success(tree.ToList());

    }
}





