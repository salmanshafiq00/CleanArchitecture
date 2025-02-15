namespace Domain.Shared;

public interface IValidationResult
{
    public static readonly Error ValidationError = new(
        "ValidationError",
        "A validation problem occured.",
        ErrorType.Validation);

    Error[] Errors { get; }
}
