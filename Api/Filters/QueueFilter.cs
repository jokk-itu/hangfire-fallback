using Hangfire.States;
using System.Reflection;

namespace Api.Filters;

public class QueueFilter : IElectStateFilter
{
    public void OnStateElection(ElectStateContext context)
    {
        if (context.CandidateState is not EnqueuedState enqueuedState)
        {
            // This filter accepts only enqueued job state.
            return;
        }

        var jobConfiguration = context.BackgroundJob.Job.Type.GetCustomAttribute<JobConfigurationAttribute>()!;
        var retryAttempt = context.GetJobParameter<int?>("RetryCount", allowStale: true);
        if (retryAttempt is null)
        {
            enqueuedState.Queue = QueueHelper.GetQueue(jobConfiguration.Queue);
        }
        else
        {
            enqueuedState.Queue = QueueHelper.GetFallbackQueue(jobConfiguration.FallbackQueue);
        }
    }
}