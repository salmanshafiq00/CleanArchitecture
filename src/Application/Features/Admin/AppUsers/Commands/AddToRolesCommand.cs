using Application.Common.Abstractions.Identity;

namespace Application.Features.Admin.AppUsers.Commands;

public record AddToRolesCommand(
     string Id,
     List<string> RoleNames
    ) : ICacheInvalidatorCommand
{
    [JsonIgnore]
    public string[] CacheKeys => [AppCacheKeys.AppUser];
}

internal sealed class AddToRolesCommandHandler(IIdentityService identityService)
    : ICommandHandler<AddToRolesCommand>
{
    public async Task<Result> Handle(AddToRolesCommand request, CancellationToken cancellationToken)
    {
        return await identityService.AddToRolesAsync(request.Id, request.RoleNames, cancellationToken);
    }
}
