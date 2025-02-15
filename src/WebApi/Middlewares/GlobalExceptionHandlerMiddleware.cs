using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace WebApi.Middlewares;

public class GlobalExceptionHandlerMiddleware(
    ILogger<GlobalExceptionHandlerMiddleware> logger)
    : IExceptionHandler
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private const string CorrelationId = "correlationId";
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)

    {
        logger.LogError(exception, "Exception occured: {Message}", exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Instance = httpContext.Request.Path
        };

        AddCorrelationIdAndException(httpContext, problemDetails, exception);

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);

        return true;
    }

    private static void AddCorrelationIdAndException(HttpContext httpContext, ProblemDetails problemDetails, Exception? exception = null)
    {
        httpContext.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId);
        problemDetails.Extensions[CorrelationId] = correlationId.FirstOrDefault() ?? httpContext.TraceIdentifier;

        var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsDevelopment() && exception is not null)
        {
            problemDetails.Extensions.Add("exception", exception.ToString());
        }
    }
}
