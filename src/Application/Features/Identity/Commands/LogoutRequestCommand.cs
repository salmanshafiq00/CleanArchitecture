using Application.Common.Abstractions.Identity;

namespace Application.Features.Identity.Commands;

public sealed record LogoutRequestCommand(
    string AccessToken,
    string RefreshToken)
    : ICommand;

internal sealed class LogoutRequestCommandHandler(IAuthService authService, IUser user)
    : ICommandHandler<LogoutRequestCommand>
{
    public async Task<Result> Handle(LogoutRequestCommand request, CancellationToken cancellationToken)
    {
        return await authService.Logout(user.Id, request.AccessToken, request.RefreshToken);
    }
}

