using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Identity.Commands;

public record ResetPasswordCommand(
    string Email,
    string Password,
    string Token) : ICommand;

internal sealed class ResetPasswordCommandHandler(
    IAuthService authService)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return await authService.ResetPasswordAsync(
            request.Email,
            request.Password,
            request.Token,
            cancellationToken);
    }
}
