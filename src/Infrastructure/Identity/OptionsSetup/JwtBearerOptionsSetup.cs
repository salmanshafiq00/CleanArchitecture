using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.OptionsSetup;

public class JwtBearerOptionsSetup(IOptions<JwtOptions> jwtOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public void Configure(string? name, JwtBearerOptions options)
    {
        options.IncludeErrorDetails = true;

        // Configure token validation parameters
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        // Configure JWT Bearer events
        options.Events = new JwtBearerEvents
        {
            //OnAuthenticationFailed = async context =>
            //{
            //    if (context.Exception is SecurityTokenExpiredException)
            //    {
            //        // Set status code first
            //        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //        context.Response.ContentType = "application/problem+json";

            //        // Create problem details
            //        var problem = new ProblemDetails
            //        {
            //            Status = StatusCodes.Status401Unauthorized,
            //            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            //            Title = "Unauthorized",
            //            Detail = "Authentication token has expired.",
            //            Instance = context.Request.Path
            //        };

            //        // Add correlation ID
            //        context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId);
            //        problem.Extensions["correlationId"] = correlationId.FirstOrDefault()
            //            ?? context.Request.HttpContext.TraceIdentifier;

            //        // Write the response
            //        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            //    }
            //},

            OnChallenge = async context =>
            {
                context.HandleResponse(); // Prevent default response

                // Set status code first
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";

                // Create problem details
                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    Title = "Unauthorized",
                    Detail = "Authentication is required to access this resource.",
                    Instance = context.Request.Path
                };

                // Add correlation ID
                context.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId);
                problem.Extensions["correlationId"] = correlationId.FirstOrDefault()
                    ?? context.Request.HttpContext.TraceIdentifier;

                // Write the response
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            },

            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // Check if this is a SignalR request
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    }
    public void Configure(JwtBearerOptions options)
    {
        Configure(Options.DefaultName, options);
    }
}
