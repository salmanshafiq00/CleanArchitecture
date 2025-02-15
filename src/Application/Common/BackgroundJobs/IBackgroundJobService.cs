using System.Linq.Expressions;

namespace Application.Common.BackgroundJobs;

public interface IBackgroundJobService
{
    // <summary>
    /// Enqueues a background job to be executed immediately.
    /// </summary>
    /// <param name="methodCall">The method to execute as a background job.</param>
    /// <returns>The job ID of the enqueued job.</returns>
    string EnqueueJob(Expression<Action> methodCall);

    /// <summary>
    /// Enqueues a background job to be executed immediately.
    /// </summary>
    /// <param name="methodCall">The method to execute as a background job.</param>
    /// <returns>The job ID of the enqueued job.</returns>
    string EnqueueJob(Expression<Func<Task>> methodCall);

    /// <summary>
    /// Enqueues a background job for a specific type to be executed immediately.
    /// </summary>
    /// <typeparam name="T">The type of the class containing the method to execute.</typeparam>
    /// <param name="methodCall">The method to execute as a background job.</param>
    /// <returns>The job ID of the enqueued job.</returns>
    string EnqueueJob<T>(Expression<Func<T, Task>> methodCall);

    /// <summary>
    /// Schedules a background job to be executed at a specified future time.
    /// </summary>
    /// <param name="methodCall">The method to execute as a background job.</param>
    /// <param name="scheduledTime">The exact time at which the job should be executed.</param>
    void ScheduleJob(Expression<Func<Task>> methodCall, DateTimeOffset scheduledTime);

    /// <summary>
    /// Sets up a recurring job that executes at intervals defined by a Cron expression.
    /// </summary>
    /// <param name="jobId">A unique identifier for the recurring job.</param>
    /// <param name="methodCall">The method to execute as a recurring job.</param>
    /// <param name="cronExpression">A function that returns a Cron expression defining the schedule.</param>
    void RecurringJob(string jobId, Expression<Func<Task>> methodCall, Func<string> cronExpression);

    // Add method for cancelling jobs
    bool CancelJob(string jobId);

    // Add method to get job status
    JobStatus GetJobStatus(string jobId);
}
