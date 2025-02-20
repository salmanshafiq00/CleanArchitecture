using System.Data;
using Application.Common.Abstractions.Identity;

namespace Application.Features.Admin.AppMenus.Queries;

public record GetSidebarMenuQuery
    : ICacheableQuery<List<SidebarMenuModel>>
{
    [JsonIgnore]
    public string CacheKey => AppCacheKeys.AppMenu_Sidebar_Tree_List;

    public bool? AllowCache => false;

    public TimeSpan? Expiration => null;
}

public class SidebarMenuModel
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string RouterLink { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool Visible { get; set; }
    public string Tooltip { get; set; } = string.Empty;
    public int OrderNo { get; set; }
    public Guid? ParentId { get; set; }
    public string ParentLabel { get; set; } = string.Empty;
    public IList<SidebarMenuModel>? Items { get; set; } = [];
}

internal sealed class GetSidebarMenuQueryHandler(ISqlConnectionFactory sqlConnection, IUser user)
    : IQueryHandler<GetSidebarMenuQuery, List<SidebarMenuModel>>
{
    public async Task<Result<List<SidebarMenuModel>>> Handle(GetSidebarMenuQuery request, CancellationToken cancellationToken)
    {
        var connection = sqlConnection.GetOpenConnection();

        // Step 1: Retrieve all menus
        var allMenusSql = $"""
            SELECT 
                M.Id AS {nameof(SidebarMenuModel.Id)}, 
                M.ParentId AS {nameof(SidebarMenuModel.ParentId)}, 
                P.Label AS {nameof(SidebarMenuModel.ParentLabel)}, 
                M.Label AS {nameof(SidebarMenuModel.Label)}, 
                M.RouterLink AS {nameof(SidebarMenuModel.RouterLink)}, 
                M.Icon AS {nameof(SidebarMenuModel.Icon)}, 
                M.Visible AS {nameof(SidebarMenuModel.Visible)},
                M.Tooltip AS {nameof(SidebarMenuModel.Tooltip)},
                M.OrderNo AS {nameof(SidebarMenuModel.OrderNo)}
            FROM dbo.AppMenus M 
            LEFT JOIN dbo.AppMenus P ON P.Id = M.ParentId 
            WHERE M.IsActive = 1
            ORDER BY M.OrderNo
            """;

        var allMenus = await connection.QueryAsync<SidebarMenuModel>(allMenusSql);

        // Step 2: Retrieve only the menus assigned to a role
        var assignedMenusSql = $"""
            SELECT 
                M.Id 
            FROM [identity].Users AS U
            INNER JOIN [identity].UserRoles UR ON UR.UserId = U.Id 
            INNER JOIN dbo.RoleMenus RM ON RM.RoleId = UR.RoleId 
            INNER JOIN dbo.AppMenus M ON M.Id = RM.AppMenuId 
            WHERE M.IsActive = 1
            AND LOWER(U.Id) = @UserId
            """;

        var assignedMenuIds = (await connection.QueryAsync<Guid>(assignedMenusSql, new { UserId = user?.Id?.ToLower() })).ToHashSet();

        // Step 3: Build the lookup dictionary for parent - child relationships
        var lookup = allMenus.ToLookup(x => x.ParentId);

        // Step 4: Recursive function to build the tree
        IList<SidebarMenuModel> BuildTree(Guid? parentId)
        {
            return lookup[parentId]
                .Where(x => assignedMenuIds.Contains(x.Id) || lookup[x.Id].Any(child => assignedMenuIds.Contains(child.Id)))
                .Select(x => new SidebarMenuModel
                {
                    Id = x.Id,
                    Label = x.Label,
                    RouterLink = x.RouterLink,
                    Icon = x.Icon,
                    Visible = x.Visible,
                    Tooltip = x.Tooltip,
                    OrderNo = x.OrderNo,
                    ParentId = x.ParentId,
                    ParentLabel = x.ParentLabel,
                    Items = BuildTree(x.Id)
                })
                .OrderBy(x => x.OrderNo)
                .ToList();
        }

        var tree = BuildTree(null);

        return Result.Success(tree.ToList());

    }
}





