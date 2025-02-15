using Application.Common.Abstractions.Identity;
using Application.Common.Abstractions.Messaging;

namespace Application.Features.Identity.Commands;

public record ForgotPasswordCommand(string Email) : ICommand;

internal sealed class ForgotPasswordCommandHandler(
    IAuthService authService)
    : ICommandHandler<ForgotPasswordCommand>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        return await authService.ForgotPasswordAsync(request.Email, cancellationToken);
    }
}
