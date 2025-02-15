using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Infrastructure.BackgroundJobs;

internal class ExpireFailedJobsAttribute : JobFilterAttribute, IApplyStateFilter
{
    private readonly TimeSpan _expirationTimeout;

    public ExpireFailedJobsAttribute(TimeSpan expirationTimeout)
    {
        _expirationTimeout = expirationTimeout;
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is FailedState)
        {
            context.JobExpirationTimeout = _expirationTimeout;
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        // No action needed on state unapplied
    }
}
