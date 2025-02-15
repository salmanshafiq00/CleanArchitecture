namespace Application.Features.Admin.Roles.Models;

public record RoleMenuModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<Guid> RoleMenus { get; set; } = [];

    public Dictionary<string, object> OptionsDataSources { get; set; } = [];

}
