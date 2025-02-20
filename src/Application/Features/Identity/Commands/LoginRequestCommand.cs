using Application.Common.Abstractions.Identity;
using Application.Features.Identity.Models;

namespace Application.Features.Identity.Commands;

public sealed record LoginRequestCommand(
    string UserName,
    string Password,
    bool IsRemember = false)
    : ICommand<AuthenticatedResponse>;

internal sealed class LoginRequestCommandHandler(IAuthService authService)
    : ICommandHandler<LoginRequestCommand, AuthenticatedResponse>
{
    public async Task<Result<AuthenticatedResponse>> Handle(LoginRequestCommand request, CancellationToken cancellationToken)
    {
        return await authService
            .LoginAsync(request.UserName, request.Password, request.IsRemember, cancellationToken);
    }
}

