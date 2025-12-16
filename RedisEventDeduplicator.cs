using StackExchange.Redis;

namespace Dedup;

public class RedisEventDeduplicator : IEventDeduplicator
{
    private readonly IDatabase _redis;
    private readonly AsyncPolicy<bool> _retryPolicy;

    private static readonly TimeSpan ProcessingTtl = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan CompletedTtl = TimeSpan.FromHours(1);

    // We are using Redis to track event processing state
 
    public RedisEventDeduplicator(IConnectionMultiplexer connection)
    {
        _redis = connection.GetDatabase();

        // Retry policy for transient Redis errors
        // This is to handle cases like temporary network issues or Redis failovers
        // We retry a few times with exponential backoff
        // Ensure not event is lost due to transient errors
        _retryPolicy = Policy<bool>
        .Handle<RedisConnectionException>()
        .Or<TimeoutException>()
        .WaitAndRetryAsync(
            retryCount: 3,
            retryAttempt => TimeSpan.FromMilliseconds(100 * retryAttempt)
        );
    }

    // When an event is being processed, we set a key with a short TTL
    public async Task<bool> TryClaimAsync(string eventId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {   
            return await _redis.StringSetAsync(
                key: GetKey(eventId),
                value: "processing",
                expiry: ProcessingTtl,
                when: When.NotExists
            );
        });
    }

    // When processing is complete, we update the key with a longer TTL
    // So redis crashes or restarts won't lead to reprocessing completed events
    public async Task MarkCompletedAsync(string eventId)
    {
        await _redis.StringSetAsync(
            key: GetKey(eventId),
            value: "completed",
            expiry: CompletedTtl
        );
    }

    private static string GetKey(string eventId)
        => $"event:{eventId}";
}
