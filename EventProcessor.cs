using Microsoft.EntityFrameworkCore;
using Models;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task SaveAsync(IncomingEvent incomingEvent)
    {
        var entity = new ProcessedEvent
        {
            EventId = incomingEvent.EventId,
            Payload = incomingEvent.Payload,
            ProcessedAt = DateTime.UtcNow
        };

        _db.ProcessedEvents.Add(entity);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Unique constraint violation → duplicate event
            throw new InvalidOperationException(
                $"Event {incomingEvent.EventId} already processed",
                ex
            );
        }
    }
}
