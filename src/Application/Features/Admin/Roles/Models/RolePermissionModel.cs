namespace Application.Features.Admin.Roles.Models;

public record RolePermissionModel
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = [];

    public Dictionary<string, object> OptionsDataSources { get; set; } = [];

}
