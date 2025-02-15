using Application.Common.Abstractions.Messaging;

namespace Application.Common.Behaviours;

internal sealed class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
     where TRequest : IBaseCommand
     where TResponse : Result
{

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next().ConfigureAwait(false);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v =>
                v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .Select(failure => new Error(
                failure.PropertyName,
                failure.ErrorMessage,
                ErrorType.Validation))
            .Distinct()
            .ToArray();

        if (errors.Length != 0)
            return CreateValidationResult<TResponse>(errors);

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(Error[] errors)
        where TResult : class
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (ValidationResult.WithErrors(errors) as TResult)!;
        }

        object validationResult = typeof(ValidationResult<>)
            .GetGenericTypeDefinition()
            .MakeGenericType(typeof(TResult).GenericTypeArguments[0])
            .GetMethod(nameof(ValidationResult.WithErrors))!
            .Invoke(null, [errors])!;

        return (TResult)validationResult;
    }
}
