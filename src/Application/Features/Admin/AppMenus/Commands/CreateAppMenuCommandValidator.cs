namespace Application.Features.Admin.AppMenus.Commands;

public class CreateAppMenuCommandValidator : AbstractValidator<CreateAppMenuCommand>
{
    private readonly ICommonQueryService _commonQuery;

    public CreateAppMenuCommandValidator(ICommonQueryService commonQuery)
    {
        _commonQuery = commonQuery;

        RuleFor(v => v.Label)
            .NotEmpty()
            .MustAsync(BeUniqueLabel)
                .WithMessage("'{PropertyName}' must be unique.")
                .WithErrorCode("Unique");

        RuleFor(v => v.MenuTypeId)
            .NotEmpty();

        RuleFor(v => v.RouterLink)
            .NotEmpty();

        RuleFor(v => v.Description)
            .MaximumLength(500)
            .WithMessage("{0} can not exceed max 500 chars.");
    }

    public async Task<bool> BeUniqueLabel(string label, CancellationToken cancellationToken)
    {
        return !await _commonQuery.IsExistAsync("dbo.AppMenus", ["Label"], new { Label = label });
    }

}
