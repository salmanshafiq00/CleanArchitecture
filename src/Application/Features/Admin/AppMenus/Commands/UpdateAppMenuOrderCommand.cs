using Application.Common.Abstractions;
using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Messaging;
using Application.Common.Constants;
using Application.Common.Models;
using Domain.Admin;

namespace Application.Features.Admin.AppMenus.Commands;

public record UpdateAppMenuOrderCommand(
    List<TreeNodeModel> ReorderedAppMenus) : ICacheInvalidatorCommand
{
    public string[] CacheKeys => [AppCacheKeys.AppMenu];
}

internal sealed class UpdateAppMenuOrderCommandHandler(
    IApplicationDbContext dbContext)
    : ICommandHandler<UpdateAppMenuOrderCommand>
{
    public async Task<Result> Handle(UpdateAppMenuOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.ReorderedAppMenus is null || request.ReorderedAppMenus.Count == 0)
        {
            return Result.Failure(Error.Failure(nameof(AppMenu), "Reordered Menus not found."));
        }
        var entities = await dbContext.AppMenus
            .ToListAsync(cancellationToken);

        if (entities is null || entities.Count == 0) return Result.Failure(Error.NotFound(nameof(entities), ErrorMessages.EntityNotFound));

        UpdateMenuOrders(request.ReorderedAppMenus, entities);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static void UpdateMenuOrders(List<TreeNodeModel> updatedMenus, List<AppMenu> entities)
    {
        for (int i = 0; i < updatedMenus.Count; i++)
        {
            var menuNode = updatedMenus[i];
            var entity = entities.FirstOrDefault(e => e.Id == menuNode.Key);

            if (entity != null)
            {
                // Update parent level menu properties
                entity.OrderNo = i; // Setting OrderNo to current index
                entity.ParentId = menuNode.Parent?.Key; // Assigning ParentId (null if no parent)

                // Update children, if any
                if (menuNode.Children != null && menuNode.Children.Any())
                {
                    // Recursive call to update children
                    UpdateMenuOrders(menuNode.Children.ToList(), entities);
                }
            }
        }
    }
}

public record AppMenuReorderModel
{
    public Guid Key { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string Data { get; set; } = string.Empty;
    public bool DisabledCheckbox { get; set; } = false;
    public bool Disabled { get; set; } = false;
    public bool Visible { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool PartialSelected { get; set; }
    public int OrderNo { get; set; }
    public bool Leaf = false;
    public TreeNodeModel? Parent { get; set; }
    public IList<TreeNodeModel> Children { get; set; } = [];
}
