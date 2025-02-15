namespace Application.Common.Abstractions.Identity;

public interface ICustomAuthorizationService
{
    Task<Result> AuthorizeAsync(string userId, string policyName, CancellationToken cancellation = default);
    Task<Result> IsInRoleAsync(string userId, string role, CancellationToken cancellation = default);

}

