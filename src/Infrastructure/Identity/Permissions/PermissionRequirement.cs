using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Identity.Permissions;
public record PermissionRequirement(string Permission) : IAuthorizationRequirement;

