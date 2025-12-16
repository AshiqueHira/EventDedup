
public interface IEventDeduplicator
{
    Task<bool> TryClaimAsync(string eventId);
    Task MarkCompletedAsync(string eventId);
}
