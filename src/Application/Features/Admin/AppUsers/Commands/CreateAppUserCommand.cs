using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Admin.AppUsers.Commands;

public record CreateAppUserCommand(
     string Username,
     string Password,
     string Email,
     string FirstName,
     string LastName,
     string PhoneNumber,
     string PhotoUrl,
     bool IsActive,
     List<string>? Roles
    ) : ICacheInvalidatorCommand<string>
{
    [JsonIgnore]
    public string[] CacheKeys => [AppCacheKeys.AppUser];
}

internal sealed class CreateAppUserCommandHandler(IIdentityService identityService) : ICommandHandler<CreateAppUserCommand, string>
{
    public async Task<Result<string>> Handle(CreateAppUserCommand request, CancellationToken cancellationToken)
    {
        return await identityService.CreateUserAsync(request, cancellationToken);
    }
}


