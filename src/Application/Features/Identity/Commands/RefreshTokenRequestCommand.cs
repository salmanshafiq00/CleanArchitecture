using Application.Common.Abstractions.Identity;
using Application.Features.Identity.Models;

namespace Application.Features.Identity.Commands;

public record RefreshTokenRequestCommand(string AccessToken, string RefreshToken)
    : ICommand<AuthenticatedResponse>;


internal sealed class RefreshTokenCommandHandler(IAuthService authService)
    : ICommandHandler<RefreshTokenRequestCommand, AuthenticatedResponse>
{
    public async Task<Result<AuthenticatedResponse>> Handle(RefreshTokenRequestCommand request, CancellationToken cancellationToken)
    {
        return await authService
            .RefreshTokenAsync(request.AccessToken, request.RefreshToken, cancellationToken);
    }
}
