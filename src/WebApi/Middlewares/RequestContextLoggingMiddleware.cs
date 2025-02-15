using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace WebApi.Middlewares;

public class RequestContextLoggingMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private const string CorrelationId = "CorrelationId";
    private readonly RequestDelegate _next;

    public RequestContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        string correlationId = GetCorrelationId(context);

        using (LogContext.PushProperty(CorrelationId, correlationId))
        {
            return _next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
