using Application.Common.DapperQueries;

namespace Application.Features.Admin.AppUsers.Commands;

internal sealed class CreateAppUserCommandValidator : AbstractValidator<CreateAppUserCommand>
{
    private readonly ICommonQueryService _commonQuery;

    public CreateAppUserCommandValidator(ICommonQueryService commonQuery)
    {
        RuleFor(x => x.FirstName)
            .NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty();

        RuleFor(x => x.Username)
            .NotEmpty()
            .MustAsync(BeUniqueUsername)
            .WithMessage("'{PropertyName}' must be unique.")
            .WithErrorCode("Unique");

        RuleFor(x => x.Username)
            .NotEmpty()
            .MustAsync(BeUniqueEmail)
            .WithMessage("'{PropertyName}' must be unique.")
            .WithErrorCode("Unique");

        RuleFor(x => x.Password)
            .NotEmpty();

        _commonQuery = commonQuery;
    }

    private async Task<bool> BeUniqueUsername(string username, CancellationToken cancellationToken)
    {
        return !await _commonQuery.IsExistAsync("[identity].Users", ["Username"], new { Username = username });
    }
    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
        return !await _commonQuery.IsExistAsync("[identity].Users", ["Email"], new { Email = email });
    }
}
