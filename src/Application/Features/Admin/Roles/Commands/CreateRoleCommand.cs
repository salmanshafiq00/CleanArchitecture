using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Admin.Roles.Commands;

public record CreateRoleCommand(
     string Name,
     List<Guid> Rolemenus,
     List<string> Permissions
    ) : ICacheInvalidatorCommand<string>
{
    [JsonIgnore]
    public string[] CacheKeys => [AppCacheKeys.Role];
}

internal sealed class CreateRoleCommandHandler(IIdentityRoleService roleService)
    : ICommandHandler<CreateRoleCommand, string>
{
    public async Task<Result<string>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        return await roleService.CreateRoleAsync(request.Name, request.Rolemenus, request.Permissions, cancellationToken);
    }
}


