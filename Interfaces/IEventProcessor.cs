public interface IEventProcessor
{
    Task ProcessAsync(IncomingEvent incomingEvent);
}
