using Domain.Shared;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public static class IdentityResultExtensions
{
    public static Result ToApplicationResult(this IdentityResult result)
    {
        return result.Succeeded
            ? Result.Success()
            : Result.Failure(MapErrors(result.Errors));
    }

    public static Result<TValue> ToApplicationResult<TValue>(this IdentityResult result, TValue value)
    {
        return result.Succeeded
            ? Result.Success(value)
            : Result.Failure<TValue>(MapErrors(result.Errors));
    }

    private static Error MapError(IdentityError identityError)
    {
        return new Error(identityError.Code, identityError.Description, ErrorType.Failure);
    }

    private static Error MapErrors(IEnumerable<IdentityError> identityErrors)
    {
        var errorDescriptions = identityErrors.Select(e => $"{e.Code}: {e.Description}").ToArray();
        var combinedErrorDescription = string.Join("; ", errorDescriptions);
        return new Error("IdentityErrors", combinedErrorDescription, ErrorType.Failure);
    }
}
