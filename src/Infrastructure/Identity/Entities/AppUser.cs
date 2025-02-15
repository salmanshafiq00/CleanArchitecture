using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Entities;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int UserType { get; set; }
    //public Guid? UserTypeId { get; set; }
    public string? PhotoUrl { get; set; } = string.Empty;

    //public IList<RefreshToken> RefreshTokens { get; private set; } = [];
}
