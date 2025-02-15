using Application.Common.DapperQueries;

namespace Application.Features.Setups.LookupDetails.Commands;

public class UpdateLookupDetailCommandValidator : AbstractValidator<UpdateLookupDetailCommand>
{
    private readonly ICommonQueryService _commonQuery;

    public UpdateLookupDetailCommandValidator(ICommonQueryService commonQuery)
    {
        _commonQuery = commonQuery;

        //RuleFor(v => v.Code)
        //  .NotEmpty()
        //  .MaximumLength(10)
        //  .MinimumLength(3)
        //  .MustAsync(async (v, code, cancellation) => await BeUniqueCodeSkipCurrent(code, v.Id, cancellation))
        //        .WithMessage("'{PropertyName}' must be unique.")
        //        .WithErrorCode("Unique");

        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200)
            .MustAsync(async (v, name, cancellation) => await BeUniqueNameSkipCurrent(name, v, cancellation))
                .WithMessage("'{PropertyName}' must be unique.")
                .WithErrorCode("Unique");

        RuleFor(v => v.Description)
            .MaximumLength(500)
            .WithMessage("{0} can not exceed max 500 chars.");

        RuleFor(v => v.LookupId)
            .NotEmpty()
            .NotNull();

    }

    public async Task<bool> BeUniqueNameSkipCurrent(string name, UpdateLookupDetailCommand command, CancellationToken cancellationToken)
    {
        return !await _commonQuery.IsExistAsync(
            "dbo.LookupDetails",
            ["Name", "LookupId"],
            new { Name = name, command.Id, command.LookupId },
            ["Id"]);
    }
    public async Task<bool> BeUniqueCodeSkipCurrent(string code, Guid id, CancellationToken cancellationToken)
    {
        return !await _commonQuery.IsExistAsync("dbo.LookupDetails", ["Code"], new { Code = code, Id = id }, ["Id"]);
    }

}
