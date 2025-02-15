using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Admin.Roles.Commands;

public record AddOrRemovePermissionCommand(
     string RoleId,
     List<string> Permissions
    ) : ICacheInvalidatorCommand
{
    [JsonIgnore]
    public string[] CacheKeys => [AppCacheKeys.Role];
}

internal sealed class AddOrRemovePermissionCommandHandler(IIdentityRoleService roleService)
    : ICommandHandler<AddOrRemovePermissionCommand>
{
    public async Task<Result> Handle(AddOrRemovePermissionCommand request, CancellationToken cancellationToken)
    {
        return await roleService.AddOrRemoveClaimsToRoleAsync(request.RoleId, request.Permissions, cancellationToken);
    }
}
