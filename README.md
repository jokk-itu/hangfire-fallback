# hangfire-fallback

Sample of Hangfire with automatic retries,
with fallback resiliency applied.
This makes sure the component causing failure, can take their time healing.

When a job fails, it applies a fallback to a less prioritized queue.
There are 10 normal queues, and 2 fallback queues.
Each job must set a "JobConfiguration" attribute at class level,
which contains the Queue, FallbackQueue, RetryAttempt and AttemptsExceeded properties.

