using Application.Common.DapperQueries;

namespace Application.Features.Setups.LookupDetails.Commands;

public class CreateLookupDetailCommandValidator : AbstractValidator<CreateLookupDetailCommand>
{
    private readonly ICommonQueryService _commonQuery;

    public CreateLookupDetailCommandValidator(ICommonQueryService commonQuery)
    {
        _commonQuery = commonQuery;

        RuleFor(v => v.Code)
            .MaximumLength(20)
              //.MinimumLength(3)
              .WithMessage("'{PropertyName}' allow max 20 characters.");

        //RuleFor(v => v.Code)
        //    .MustAsync(BeUniqueCode)
        //        .WithMessage("'{PropertyName}' must be unique.")
        //        .WithErrorCode("Unique");

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200)
            .MustAsync(async (command, name, cancellationToken) => await BeUniqueName(name, command.LookupId, cancellationToken))
                .WithMessage("'{PropertyName}' must be unique.")
                .WithErrorCode("Unique");

        RuleFor(v => v.Description)
            .MaximumLength(500)
            .WithMessage("{0} can not exceed max 500 chars.");
    }

    public async Task<bool> BeUniqueName(string name, Guid lookupId, CancellationToken cancellationToken = default)
    {
        return !await _commonQuery.IsExistAsync("dbo.LookupDetails", ["Name", "lookupId"], new { Name = name, LookupId = lookupId });
    }
    public async Task<bool> BeUniqueCode(string code, CancellationToken cancellationToken = default)
    {
        return !await _commonQuery.IsExistAsync("dbo.LookupDetails", ["Code"], new { Code = code });
    }

}
