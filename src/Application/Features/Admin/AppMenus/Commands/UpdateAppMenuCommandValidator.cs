using Application.Common.DapperQueries;

namespace Application.Features.Admin.AppMenus.Commands;

public class UpdateAppMenuCommandValidator : AbstractValidator<UpdateAppMenuCommand>
{
    private readonly ICommonQueryService _commonQuery;

    public UpdateAppMenuCommandValidator(ICommonQueryService commonQuery)
    {
        _commonQuery = commonQuery;

        RuleFor(v => v.Label)
          .NotEmpty()
          .MustAsync(async (v, label, cancellation) => await BeUniqueLabelSkipCurrent(label, v.Id, cancellation))
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

    public async Task<bool> BeUniqueLabelSkipCurrent(string label, Guid id, CancellationToken cancellationToken)
    {
        return !await _commonQuery.IsExistAsync("dbo.AppMenus", ["Label"], new { Label = label, Id = id }, ["Id"]);
    }

}
