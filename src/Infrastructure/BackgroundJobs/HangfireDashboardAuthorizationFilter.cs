using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Infrastructure.BackgroundJobs;

public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private const string RequiredRole = "Administrator";
    public bool Authorize([NotNull] DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole(RequiredRole);
    }
}
