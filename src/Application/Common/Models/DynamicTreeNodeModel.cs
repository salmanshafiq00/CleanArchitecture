namespace Application.Common.Models;

public class DynamicTreeNodeModel
{
    public dynamic Key { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public dynamic? ParentId { get; set; }
    public string Data { get; set; } = string.Empty;
    public bool DisabledCheckbox { get; set; } = false;
    public bool Disabled { get; set; } = false;
    public bool Visible { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool PartialSelected { get; set; }
    public int OrderNo { get; set; }
    public bool Leaf => Children.Count == 0;
    public DynamicTreeNodeModel? Parent { get; set; }
    public IList<DynamicTreeNodeModel> Children { get; set; } = [];
}

