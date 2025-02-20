using Application.Common.Abstractions.Identity;

namespace Application.Features.Admin.AppUsers.Commands;

public record ChangeUserPhotoCommand(string PhotoUrl
    ) : ICacheInvalidatorCommand
{
    [JsonIgnore]
    public string[] CacheKeys => [AppCacheKeys.AppUser];
}

internal sealed class ChangeUserPhotoCommandHandler(IIdentityService identityService, IUser user)
    : ICommandHandler<ChangeUserPhotoCommand>
{
    public async Task<Result> Handle(ChangeUserPhotoCommand request, CancellationToken cancellationToken)
    {
        return await identityService.ChangePhotoAsync(user.Id, request.PhotoUrl, cancellationToken);
    }
}
