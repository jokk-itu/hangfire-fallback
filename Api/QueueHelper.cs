namespace Api;

public static class QueueHelper
{
    public static string GetQueue(Queue queue)
    {
        return queue switch
        {
            Queue.Priority10 => QueueConstants.Priority10,
            Queue.Priority20 => QueueConstants.Priority20,
            Queue.Priority30 => QueueConstants.Priority30,
            Queue.Priority40 => QueueConstants.Priority40,
            Queue.Priority50 => QueueConstants.Priority50,
            Queue.Priority60 => QueueConstants.Priority60,
            Queue.Priority70 => QueueConstants.Priority70,
            Queue.Priority80 => QueueConstants.Priority80,
            Queue.Priority90 => QueueConstants.Priority90,
            _ => throw new InvalidOperationException($"Unsupported queue {queue}")
        };
    }

    public static string GetFallbackQueue(FallbackQueue fallbackQueue)
    {
        return fallbackQueue switch
        {
            FallbackQueue.Priority50 => QueueConstants.Priority50Fallback,
            FallbackQueue.Priority90 => QueueConstants.Priority90Fallback,
            _ => throw new InvalidOperationException($"Unsupported queue {fallbackQueue}")
        };
    }
}
