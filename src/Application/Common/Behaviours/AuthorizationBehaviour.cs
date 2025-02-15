using System.Reflection;
using Application.Common.Abstractions.Identity;

namespace Application.Common.Behaviours;

internal sealed class AuthorizationBehaviour<TRequest, TResponse>(
    IUser user,
    ICustomAuthorizationService identityService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var authorizeAttributes = request.GetType().GetCustomAttributes<AuthorizeAttribute>();

        if (!authorizeAttributes.Any())
        {
            return await next().ConfigureAwait(false);
        }

        // Authentication check
        if (user.Id == null)
        {
            return CreateAuthorizationResult<TResponse>(
                Error.Unauthorized(
                    "Authentication.Required",
                    "Authentication is required to access this resource."));
        }

        // Role-based authorization
        var authorizeAttributesWithRoles = authorizeAttributes.Where(a =>
            !string.IsNullOrWhiteSpace(a.Roles));

        if (authorizeAttributesWithRoles.Any())
        {
            var roleAuthorizationResult = await CheckRoleAuthorizationAsync(
                authorizeAttributesWithRoles,
                user.Id,
                cancellationToken);

            if (roleAuthorizationResult.IsFailure)
            {
                return CreateAuthorizationResult<TResponse>(roleAuthorizationResult.Error);
            }
        }

        // Policy-based authorization
        var authorizeAttributesWithPolicies = authorizeAttributes
            .Where(a => !string.IsNullOrWhiteSpace(a.Policy));

        if (authorizeAttributesWithPolicies.Any())
        {
            var policyAuthorizationResult = await CheckPolicyAuthorizationAsync(
                authorizeAttributesWithPolicies,
                user.Id,
                cancellationToken);

            if (policyAuthorizationResult.IsFailure)
            {
                return CreateAuthorizationResult<TResponse>(policyAuthorizationResult.Error);
            }
        }

        // User is authorized / authorization not required
        return await next().ConfigureAwait(false);
    }

    private async Task<Result> CheckRoleAuthorizationAsync(
        IEnumerable<AuthorizeAttribute> authorizeAttributesWithRoles,
        string userId,
        CancellationToken cancellationToken)
    {
        foreach (var roles in authorizeAttributesWithRoles.Select(a => a.Roles.Split(',')))
        {
            foreach (var role in roles)
            {
                var isInRole = await identityService.IsInRoleAsync(
                    userId,
                    role.Trim(),
                    cancellationToken);

                if (isInRole.IsSuccess)
                {
                    return Result.Success();
                }
            }
        }

        return Result.Failure(Error.Forbidden(
            "Authorization.InvalidRole",
            "User does not have the required roles to access this resource."));
    }

    private async Task<Result> CheckPolicyAuthorizationAsync(
        IEnumerable<AuthorizeAttribute> authorizeAttributesWithPolicies,
        string userId,
        CancellationToken cancellationToken)
    {
        foreach (var policy in authorizeAttributesWithPolicies.Select(a => a.Policy))
        {
            var authorized = await identityService.AuthorizeAsync(
                userId,
                policy,
                cancellationToken);

            if (authorized.IsFailure)
            {
                return Result.Failure(Error.Forbidden(
                    "Authorization.PolicyViolation",
                    $"User does not satisfy the authorization policy: {policy}"));
            }
        }

        return Result.Success();
    }

    private static TResult CreateAuthorizationResult<TResult>(Error error)
        where TResult : class
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Failure(error) as TResult)!;
        }

        var genericArgument = typeof(TResult).GenericTypeArguments[0];

        // Get the specific method using both the name and parameter types
        var method = typeof(Result)
            .GetMethods()
            .First(m =>
                m.Name == nameof(Result.Failure) &&
                m.IsGenericMethod &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(Error));

        var genericMethod = method.MakeGenericMethod(genericArgument);
        var result = genericMethod.Invoke(null, [error])!;
        return (TResult)result;
    }
}
