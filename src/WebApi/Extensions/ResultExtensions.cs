namespace WebApi.Extensions;

public static class ResultExtensions
{
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess) throw new InvalidOperationException("Cannot convert successful result to problem details");

        if (result is IValidationResult validationResult)
        {
            return Results.Problem(
                statusCode: GetStatusCode(result.Error.ErrorType),
                type: GetProblemType(result.Error.ErrorType),
                title: "Validation Error",
                detail: result.Error.Description,
                extensions: new Dictionary<string, object?>
                {
                      {"errors", validationResult.Errors }
                });
        }

        return Results.Problem(
            statusCode: GetStatusCode(result.Error.ErrorType),
            type: GetProblemType(result.Error.ErrorType),
            //title: GetTitle(result.Error.ErrorType),
            title: result.Error.Code,
            detail: result.Error.Description,
            extensions: new Dictionary<string, object?>
            {
                {"errors", new[] { result.Error } }
            });
    }

    private static int GetStatusCode(ErrorType errorType) =>
            errorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Critical => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };


    private static string GetTitle(ErrorType errorType) =>
     errorType switch
     {
         ErrorType.Validation => "Bad Request",
         ErrorType.Unauthorized => "Unauthorized",
         ErrorType.Forbidden => "Forbidden",
         ErrorType.NotFound => "Not Found",
         ErrorType.Conflict => "Conflict",
         _ => "Internal Server Error"
     };

    private static string GetProblemType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
            ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

}
