using Application.Common.Models;

namespace Application.Common.Extensions;

public static class TreeBuilder
{
    public static IList<TreeNodeModel> BuildTree<T>(
            IEnumerable<T> items,
            Func<T, Guid?> parentIdSelector,
            Func<T, TreeNodeModel> mapFunc)
    {
        var lookup = items.ToLookup(parentIdSelector);

        IList<TreeNodeModel> Build(Guid? parentId)
        {
            return lookup[parentId]
                .Select(x =>
                {
                    var node = mapFunc(x);
                    node.Children = Build(node.Key);
                    return node;
                })
                .OrderBy(x => x.OrderNo)
                .ToList();
        }

        return Build(null);
    }
}
