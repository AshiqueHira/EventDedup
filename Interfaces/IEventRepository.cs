

public interface IEventRepository
{
    Task SaveAsync(IncomingEvent incomingEvent);
}
