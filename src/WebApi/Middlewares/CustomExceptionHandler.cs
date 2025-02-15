using DocumentFormat.OpenXml.InkML;
using Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace WebApi.Middlewares;

public class CustomExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;
    private readonly ILogger<CustomExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private const string CorrelationId = "correlationId";

    public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger, IWebHostEnvironment env)
    {
        _exceptionHandlers = new()
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnauthorizedAccessException), HandleUnauthorizedAccessException },
            { typeof(ForbiddenAccessException), HandleForbiddenAccessException },
        };
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (_exceptionHandlers.TryGetValue(exception.GetType(), out var handler))
        {
            await handler(httpContext, exception);
            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
            return true;
        }

        return false;
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problemDetails = new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc7807",
            Title = "Validation Error",
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        AddCorrelationIdAndException(httpContext, problemDetails, ex);
        await httpContext.Response.WriteAsJsonAsync(problemDetails);
    }

    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        var exception = (NotFoundException)ex;
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Not Found",
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        AddCorrelationIdAndException(httpContext, problemDetails, ex);
        await httpContext.Response.WriteAsJsonAsync(problemDetails);
    }

    private async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        httpContext.Response.Headers.WWWAuthenticate = "Bearer";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Detail = "Authentication is required to access this resource.",
            Instance = httpContext.Request.Path
        };

        AddCorrelationIdAndException(httpContext, problemDetails, ex);
        await httpContext.Response.WriteAsJsonAsync(problemDetails);
    }

    private async Task HandleForbiddenAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Detail = "You do not have the required permissions to access this resource.",
            Instance = httpContext.Request.Path
        };

        AddCorrelationIdAndException(httpContext, problemDetails, ex);
        await httpContext.Response.WriteAsJsonAsync(problemDetails);
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
