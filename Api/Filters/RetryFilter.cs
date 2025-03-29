using System.Reflection;
using Hangfire;
using Hangfire.States;
using Hangfire.Storage;

namespace Api.Filters;

public class RetryFilter : IElectStateFilter, IApplyStateFilter
{
    /// <summary>
    /// Makes sure the minimum delay is between thirty and sixty minutes.
    /// Multiply that by the current retry attempt.
    /// </summary>
    private static readonly Func<int, int> DefaultDelayInSecondsByAttemptFunc =
        attempt => Random.Shared.Next(1800, 3600) * attempt;

    /// <summary>
    /// Custom implementation from https://github.com/HangfireIO/Hangfire/blob/main/src/Hangfire.Core/AutomaticRetryAttribute.cs
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState is not FailedState failedState)
        {
            // This filter accepts only failed job state.
            return;
        }

        var jobConfiguration = context.BackgroundJob.Job.Type.GetCustomAttribute<JobConfigurationAttribute>()!;
        
        var retryAttempt = context.GetJobParameter<int>("RetryCount", allowStale: true) + 1;
        if (retryAttempt <= jobConfiguration.RetryAttempts)
        {
            ScheduleAgainLater(context, retryAttempt, jobConfiguration);
        }
        else if (retryAttempt > jobConfiguration.RetryAttempts && jobConfiguration.AttemptsExceeded == AttemptsExceeded.Delete)
        {
            TransitionToDeleted(context, failedState, jobConfiguration);
        }
    }

    /// <summary>
    /// Custom implementation from https://github.com/HangfireIO/Hangfire/blob/main/src/Hangfire.Core/AutomaticRetryAttribute.cs
    /// </summary>
    /// <param name="context"></param>
    /// <param name="transaction"></param>
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is ScheduledState &&
            context.NewState.Reason != null &&
            context.NewState.Reason.StartsWith("Retry attempt", StringComparison.OrdinalIgnoreCase))
        {
            transaction.AddToSet("retries", context.BackgroundJob.Id);
        }
    }

    /// <summary>
    /// Custom implementation from https://github.com/HangfireIO/Hangfire/blob/main/src/Hangfire.Core/AutomaticRetryAttribute.cs
    /// </summary>
    /// <param name="context"></param>
    /// <param name="transaction"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (ScheduledState.StateName.Equals(context.OldStateName, StringComparison.OrdinalIgnoreCase) ||
            FailedState.StateName.Equals(context.OldStateName, StringComparison.OrdinalIgnoreCase))
        {
            transaction.RemoveFromSet("retries", context.BackgroundJob.Id);
        }
    }

    private static void TransitionToDeleted(ElectStateContext context, FailedState failedState, JobConfigurationAttribute jobConfiguration)
    {
        context.CandidateState = new DeletedState(new ExceptionInfo(failedState.Exception))
        {
            Reason = jobConfiguration.RetryAttempts > 0
                ? "Exceeded the maximum number of retry attempts."
                : "Retries were disabled for this job."
        };
    }

    private static void ScheduleAgainLater(ElectStateContext context, int retryAttempt, JobConfigurationAttribute jobConfiguration)
    {
        context.SetJobParameter("RetryCount", retryAttempt);

        var delayInSeconds = DefaultDelayInSecondsByAttemptFunc(retryAttempt);
        var delay = TimeSpan.FromSeconds(delayInSeconds);

        var reason = $"Retry attempt {retryAttempt} of {jobConfiguration.RetryAttempts}";

        context.CandidateState = delay == TimeSpan.Zero
            ? new EnqueuedState { Reason = reason }
            : new ScheduledState(delay) { Reason = reason };
    }
}