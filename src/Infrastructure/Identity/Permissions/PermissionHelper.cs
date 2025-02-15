using System.Reflection;
using Application.Common.Models;

namespace Infrastructure.Identity.Permissions;

public static class PermissionHelper
{
    public static void GetAllConstantPermissions(this List<string> permissions, Type policy)
    {
        FieldInfo[] fields = policy.GetFields(BindingFlags.Static | BindingFlags.Public);

        foreach (FieldInfo field in fields)
        {
            permissions.Add(field.GetValue(null).ToString());
        }
    }

    public static IList<DynamicTreeNodeModel> MapPermissionsToTree()
    {
        var rootType = typeof(Application.Common.Security.Permissions);
        var treeNodes = new List<DynamicTreeNodeModel>();
        var rootNode = new DynamicTreeNodeModel
        {
            Key = "Permissions",
            Label = "Permissions",
            Children = []
        };

        TraversePermissions(rootType, rootNode.Children, rootNode.Key);
        treeNodes.Add(rootNode);
        return treeNodes;
    }

    private static void TraversePermissions(Type type, IList<DynamicTreeNodeModel> nodes, string parentKey)
    {
        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            var node = new DynamicTreeNodeModel
            {
                Key = nestedType.Name,
                Label = nestedType.Name,
                ParentId = parentKey,
                Children = []
            };

            TraversePermissions(nestedType, node.Children, node.Key);

            foreach (var field in nestedType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var childNode = new DynamicTreeNodeModel
                {
                    Key = field.GetValue(null).ToString(),
                    Label = field.GetValue(null).ToString(),
                    ParentId = node.Key,
                    IsActive = true // or some logic to determine if it's active
                };
                node.Children.Add(childNode);
            }

            nodes.Add(node);
        }
    }
}
