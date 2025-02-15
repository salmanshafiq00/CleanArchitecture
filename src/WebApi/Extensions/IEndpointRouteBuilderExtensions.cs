using WebApi.Extensions;

namespace WebApi.Extensions;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapGet(this IEndpointRouteBuilder builder, Delegate handler, string pattern = "", string? withName = null)
    {
        Guard.Against.AnonymousMethod(handler);

        builder.MapGet(pattern, handler)
             .WithName(withName ?? handler.Method.Name);

        return builder;
    }

    public static IEndpointRouteBuilder MapPost(this IEndpointRouteBuilder builder, Delegate handler, string pattern = "", string? withName = null)
    {
        Guard.Against.AnonymousMethod(handler);

        builder.MapPost(pattern, handler)
            .WithName(withName ?? handler.Method.Name);

        return builder;
    }

    public static IEndpointRouteBuilder MapPut(this IEndpointRouteBuilder builder, Delegate handler, string pattern = "", string? withName = null)
    {
        Guard.Against.AnonymousMethod(handler);

        builder.MapPut(pattern, handler)
            .WithName(withName ?? handler.Method.Name);

        return builder;
    }

    public static IEndpointRouteBuilder MapDelete(this IEndpointRouteBuilder builder, Delegate handler, string pattern, string? withName = null)
    {
        Guard.Against.AnonymousMethod(handler);

        builder.MapDelete(pattern, handler)
            .WithName(withName ?? handler.Method.Name);

        return builder;
    }
}
