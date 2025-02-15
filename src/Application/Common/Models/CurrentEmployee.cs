namespace Application.Common.Models;

public class CurrentEmployee
{
    public Guid? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Code { get; set; }
    public string? PhotoUrl { get; set; }
    public string? UserId { get; set; }
}
