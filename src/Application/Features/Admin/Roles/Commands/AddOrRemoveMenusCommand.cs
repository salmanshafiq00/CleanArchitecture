using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Admin.Roles.Commands;

public record AddOrRemoveMenusCommand(
     string RoleId,
     List<Guid> RoleMenus
    ) : ICacheInvalidatorCommand
{
    [JsonIgnore]
    public string[] CacheKeys => [AppCacheKeys.Role];
}

internal sealed class AddOrRemoveMenusCommandHandler(IIdentityRoleService roleService)
    : ICommandHandler<AddOrRemoveMenusCommand>
{
    public async Task<Result> Handle(AddOrRemoveMenusCommand request, CancellationToken cancellationToken)
    {
        return await roleService.RemoveAndAddRoleMenuAsync(request.RoleId, request.RoleMenus, cancellationToken);
    }
}
