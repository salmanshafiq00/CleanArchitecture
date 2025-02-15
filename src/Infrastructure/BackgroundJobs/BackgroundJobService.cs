using System.Linq.Expressions;
using Application.Common.BackgroundJobs;
using Hangfire;

namespace Infrastructure.BackgroundJobs;

internal sealed class BackgroundJobService(ILogger<BackgroundJobService> logger) : IBackgroundJobService
{
    public string EnqueueJob(Expression<Action> methodCall)
    {
        try
        {
            return BackgroundJob.Enqueue(methodCall);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue job");
            throw;
        }
    }

    public string EnqueueJob(Expression<Func<Task>> methodCall)
    {
        try
        {
            return BackgroundJob.Enqueue(methodCall);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue job");
            throw;
        }
    }

    public string EnqueueJob<T>(Expression<Func<T, Task>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    public void ScheduleJob(Expression<Func<Task>> methodCall, DateTimeOffset scheduledTime)
    {
        BackgroundJob.Schedule(methodCall, scheduledTime);
    }

    public void RecurringJob(string jobId, Expression<Func<Task>> methodCall, Func<string> cronExpression)
    {
        Hangfire.RecurringJob.AddOrUpdate(
            jobId,
            methodCall,
            cronExpression()
        );
    }


    public bool CancelJob(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }

    public JobStatus GetJobStatus(string jobId)
    {
        var jobStorage = JobStorage.Current;
        using var connection = jobStorage.GetConnection();
        var jobData = connection.GetJobData(jobId);
        return jobData?.State switch
        {
            "Enqueued" => JobStatus.Pending,
            "Processing" => JobStatus.Running,
            "Succeeded" => JobStatus.Completed,
            "Failed" => JobStatus.Failed,
            _ => JobStatus.Unknown
        };
    }

}
