namespace EmailService.Domain
{
    public interface IEventStoreHandler
    {
        Task<EventLog?> GetEventFromTypeByMessageId(string type, Guid id);
        Task AddEventAsync(EventLog log);
    }
}
