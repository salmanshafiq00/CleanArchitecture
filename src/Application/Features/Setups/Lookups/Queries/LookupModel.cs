using Microsoft.AspNetCore.Http;

namespace Application.Features.Setups.Lookups.Queries;

public record LookupModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public DateTime? Created { get; set; }
    public DateOnly? CreatedDate { get; set; }
    public TimeOnly? CreatedTime { get; set; }
    public int? CreatedYear { get; set; }
    public List<string> Subjects { get; set; } = [];
    public string SubjectRadio { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public IFormFile UploadFile { get; set; }
    public string DescEdit { get; set; } = string.Empty;
    public List<Guid> Menus { get; set; } = [];
    public Guid SingleMenu { get; set; }
    public List<Guid> TreeSelectMenus { get; set; } = [];
    public Guid TreeSelectSingleMenu { get; set; }

    public Dictionary<string, object> OptionsDataSources { get; set; } = [];
}
