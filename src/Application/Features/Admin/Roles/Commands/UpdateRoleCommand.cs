using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Admin.Roles.Commands;

public record UpdateRoleCommand(
     string Id,
     string Name,
     List<Guid> Rolemenus,
     List<string> Permissions
    ) : ICacheInvalidatorCommand
{
    [JsonIgnore]
    public string[] CacheKeys => [AppCacheKeys.Role];
}

internal sealed class UpdateRoleCommandHandler(IIdentityRoleService roleService)
    : ICommandHandler<UpdateRoleCommand>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        return await roleService.UpdateRoleAsync(
            request.Id,
            request.Name,
            request.Rolemenus,
            request.Permissions,
            cancellationToken);
    }
}
