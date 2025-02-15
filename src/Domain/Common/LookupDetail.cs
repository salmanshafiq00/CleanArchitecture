using Domain.Abstractions;

namespace Domain.Common;

public class LookupDetail : BaseAuditableEntity
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public bool Status { get; set; }
    public Guid? ParentId { get; set; }
    public Guid LookupId { get; set; }
    public int? DevCode { get; set; }


    public virtual LookupDetail Parent { get; set; } = default!;
    public virtual Lookup Lookup { get; set; } = default!;
}
