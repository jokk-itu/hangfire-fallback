namespace Api;

[AttributeUsage(AttributeTargets.Class)]
public class JobConfigurationAttribute : Attribute
{
    public Queue Queue { get; private init; }
    public FallbackQueue FallbackQueue { get; private init; }
    public uint RetryAttempts { get; private init; }
    public AttemptsExceeded AttemptsExceeded { get; private init; }

    public JobConfigurationAttribute(Queue queue, FallbackQueue fallbackQueue, uint retryAttempts, AttemptsExceeded attemptsExceeded)
    {
        Queue = queue;
        FallbackQueue = fallbackQueue;
        RetryAttempts = retryAttempts;
        AttemptsExceeded = attemptsExceeded;
    }

    public JobConfigurationAttribute(Queue queue, FallbackQueue fallbackQueue, uint retryAttempts)
        : this(queue, fallbackQueue, retryAttempts, AttemptsExceeded.Fail) { }


    public JobConfigurationAttribute(Queue queue, FallbackQueue fallbackQueue, AttemptsExceeded attemptsExceeded)
        : this(queue, fallbackQueue, 4, attemptsExceeded) { }

    public JobConfigurationAttribute(Queue queue, FallbackQueue fallbackQueue)
        : this(queue, fallbackQueue, 4, AttemptsExceeded.Fail) { }
}