namespace Api;

[JobConfiguration(Queue.Priority10, FallbackQueue.Priority50)]
public class SuccessJob : IFireAndForgetJob<SuccessContext>
{
    private readonly ILogger<SuccessJob> _logger;

    public SuccessJob(ILogger<SuccessJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(SuccessContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request with Id {Id}", context.Id);
        return Task.CompletedTask;
    }
}