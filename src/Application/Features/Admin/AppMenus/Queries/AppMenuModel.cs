namespace Application.Features.Admin.AppMenus.Queries;

public record AppMenuModel
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string RouterLink { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int OrderNo { get; set; }
    public bool Visible { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Active { get; set; } = string.Empty;
    public Guid MenuTypeId { get; set; }
    public string MenuTypeName { get; set; } = string.Empty;
    public Dictionary<string, object> OptionsDataSources { get; set; } = [];

}
