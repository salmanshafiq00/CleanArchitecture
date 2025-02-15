using Domain.Abstractions;

namespace Domain.Admin;

public class AppMenu : BaseAuditableEntity
{
    public string Label { get; set; }
    public string? RouterLink { get; set; }
    public string? Icon { get; set; }
    public Guid? ParentId { get; set; }
    public string? Tooltip { get; set; }
    public bool IsActive { get; set; }
    public int OrderNo { get; set; }
    public bool Visible { get; set; }
    public Guid MenuTypeId { get; set; }
    public string? Description { get; set; }

}
