using Microsoft.Extensions.Logging;
using Dedup;
using Persistence;
using Models;

namespace Listener;

public class WebSocketEventListener
{
    private readonly IEventDeduplicator _deduplicator;
    private readonly IEventRepository _repository;
    private readonly ILogger<WebSocketEventListener> _logger;

    public WebSocketEventListener(
        IEventDeduplicator deduplicator,
        IEventRepository repository,
        ILogger<WebSocketEventListener> logger)
    {
        _deduplicator = deduplicator;
        _repository = repository;
        _logger = logger;
    }

    
    public async Task HandleAsync(IncomingEvent incomingEvent)
    {
        // Here is the event is identified by incomingEvent.EventId
        // Here we assume the event has an EventId property
        var eventId = incomingEvent.EventId;

        // After receiving an event, first try to claim it for processing
        // if claiming fails, it means another instance is processing it
        if (!await _deduplicator.TryClaimAsync(eventId))
        {
            _logger.LogInformation(
                "Event {EventId} already being processed",
                eventId
            );
            return;
        }

        // 
        try
        {
            await _repository.SaveAsync(incomingEvent);

            await _deduplicator.MarkCompletedAsync(eventId);

            _logger.LogInformation(
                "Event {EventId} processed successfully",
                eventId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Processing failed for event {EventId}",
                eventId
            );

            // Let TTL expire → retry possible
            throw;
        }
    }
}
