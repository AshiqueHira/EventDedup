using System.ComponentModel.DataAnnotations;

namespace Persistence;

public class ProcessedEvent
{
    public int Id { get; set; }

    [Required]
    public string EventId { get; set; } = default!;

    [Required]
    public string Payload { get; set; } = default!;

    public DateTime ProcessedAt { get; set; }
}
