using Domain.Abstractions;

namespace Domain.Admin;

public class RoleMenu : BaseEntity
{
    public string RoleId { get; set; }
    public Guid AppMenuId { get; set; }
}
