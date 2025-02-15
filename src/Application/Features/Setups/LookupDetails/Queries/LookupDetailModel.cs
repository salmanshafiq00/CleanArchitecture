namespace Application.Features.Setups.LookupDetails.Queries;

public record LookupDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public Guid? LookupId { get; set; }
    public string LookupName { get; set; } = string.Empty;
    public Dictionary<string, object> OptionsDataSources { get; set; } = [];
}
