using System.Text.Json;
using Application.Common.Abstractions.Identity;
using NSwag;
using NSwag.Generation.Processors.Security;
using WebApi.Middlewares;
using WebApi.Services;
using ZymLabs.NSwag.FluentValidation;

namespace WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllerWithJsonConfiguration();
        services.AddCorsPolicy(configuration);
        services.AddCustomProblemDetails();
        services.AddOpenApiDocumentConfig();

        services.AddHttpContextAccessor();
        services.AddScoped<IUser, CurrentUser>();
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddExceptionHandlers();
        services.AddRazorPages();

        services.AddFluentValidationSchemaProcessor();
        services.AddEndpointsApiExplorer();

        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        return services;
    }
    private static void AddControllerWithJsonConfiguration(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        });
    }

    private static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(ApiConstants.AllowOriginPolicy, builder =>
            {
                builder.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            });
        });
    }

    public static void AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = (context) =>
            {
                var problemDetails = context.ProblemDetails;
                var httpContext = context.HttpContext;

                // Add correlation ID
                var correlationId = httpContext.Request.Headers.TryGetValue(ApiConstants.CorrelationIdHeaderName, out var correlationHeader)
                    ? correlationHeader.FirstOrDefault()
                    : httpContext.TraceIdentifier;

                problemDetails.Instance = httpContext.Request.Path;
                problemDetails.Extensions.Add(ApiConstants.CorrelationId, correlationId);

                // If in development, add more detailed information
                var env = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                if (env.IsDevelopment() && context.Exception is not null)
                {
                    problemDetails.Extensions.Add("exception", context.Exception.ToString());
                }
            };
        });
    }

    private static void AddOpenApiDocumentConfig(this IServiceCollection services)
    {
        services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "EasyPOS API";

            // Add the fluent validations schema processor
            var fluentValidationSchemaProcessor =
                sp.CreateScope().ServiceProvider.GetRequiredService<FluentValidationSchemaProcessor>();

            configure.SchemaSettings.SchemaProcessors.Add(fluentValidationSchemaProcessor);

            // Add JWT
            configure.AddSecurity("JWT", [], new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });
    }

    private static void AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
    }

    private static void AddFluentValidationSchemaProcessor(this IServiceCollection services)
    {
        services.AddScoped(provider =>
        {
            var validationRules = provider.GetService<IEnumerable<FluentValidationRule>>();
            var loggerFactory = provider.GetService<ILoggerFactory>();

            return new FluentValidationSchemaProcessor(provider, validationRules, loggerFactory);
        });
    }

}
