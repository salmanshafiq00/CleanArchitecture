using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Admin.Roles.Commands;

public record DeleteRoleCommand(string Id) : ICacheInvalidatorCommand
{
    public string[] CacheKeys => [AppCacheKeys.Role];
}

internal sealed class DeleteRoleCommandHandler(
    IIdentityRoleService roleService)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        return await roleService.DeleteRoleAsync(request.Id, cancellationToken);

    }
}
