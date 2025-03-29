namespace Api;

[JobConfiguration(Queue.Priority70, FallbackQueue.Priority90, 2)]
public class FailJob : IFireAndForgetJob<FailContext>
{
    public Task Execute(FailContext context, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException($"Fail on purpose for Id {context.Id}");
    }
}