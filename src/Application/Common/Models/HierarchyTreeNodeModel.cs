namespace Application.Common.Models;

public class HierarchyTreeNodeModel
{
    public Guid Key { get; set; }
    public bool Expanded { get; set; } = true;
    public string Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public object Data { get; set; } = string.Empty;
    public bool DisabledCheckbox { get; set; } = false;
    public bool Disabled { get; set; } = false;
    public bool Visible { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool PartialSelected { get; set; }
    public int OrderNo { get; set; }
    public bool Leaf => Children.Count == 0;
    public HierarchyTreeNodeModel? Parent { get; set; }
    public IList<HierarchyTreeNodeModel> Children { get; set; } = [];
}

