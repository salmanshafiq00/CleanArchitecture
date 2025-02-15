using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Identity.Commands;

public record ChangePasswordCommand(
     string CurrentPassword,
     string NewPassword
    ) : ICommand
{
}

internal sealed class ChangePasswordCommandHandler(
    IAuthService authService,
    IUser user)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        return await authService.ChangePasswordAsync(
            user.Id,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);
    }
}
