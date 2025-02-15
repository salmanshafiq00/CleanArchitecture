using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Admin.AppUsers.Commands;

public record UpdateAppUserCommand(
     string Id,
     string Username,
     string Email,
     string FirstName,
     string LastName,
     string PhoneNumber,
     bool IsActive,
     List<string>? Roles
    ) : ICacheInvalidatorCommand
{
    [JsonIgnore]
    public string[] CacheKeys => [AppCacheKeys.AppUser];
}

internal sealed class UpdateAppUserCommandHandler(IIdentityService identityService)
    : ICommandHandler<UpdateAppUserCommand>
{
    public async Task<Result> Handle(UpdateAppUserCommand request, CancellationToken cancellationToken)
    {
        return await identityService.UpdateUserAsync(request, cancellationToken);
    }
}
