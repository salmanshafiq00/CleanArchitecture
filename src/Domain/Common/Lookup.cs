using Domain.Abstractions;

namespace Domain.Common;

public class Lookup : BaseAuditableEntity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public bool Status { get; set; }
    public int? DevCode { get; set; }
    public Guid? ParentId { get; set; }

    public virtual Lookup Parent { get; set; } = default!;
}
