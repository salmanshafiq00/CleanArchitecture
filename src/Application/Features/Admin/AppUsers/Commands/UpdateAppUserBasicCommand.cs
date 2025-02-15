using Application.Common.Abstractions.Caching;
using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Admin.AppUsers.Commands;

public record UpdateAppUserBasicCommand(
     string Id,
     string Email,
     string FirstName,
     string LastName,
     string PhoneNumber
    ) : ICacheInvalidatorCommand
{
    [JsonIgnore]
    public string[] CacheKeys => [$"{AppCacheKeys.AppUser}:{Id}"];
}

internal sealed class UpdateAppUserBasicCommandHandler(IIdentityService identityService)
    : ICommandHandler<UpdateAppUserBasicCommand>
{
    public async Task<Result> Handle(UpdateAppUserBasicCommand request, CancellationToken cancellationToken)
    {
        return await identityService.UpdateUserBasicAsync(request, cancellationToken);
    }
}
