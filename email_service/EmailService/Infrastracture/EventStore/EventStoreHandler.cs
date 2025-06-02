using EmailService.Domain;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace EmailService.Infrastracture.EventStore
{
    public class EventStoreHandler : IEventStoreHandler
    {
        private readonly IMongoCollection<EventLog> _context;

        public EventStoreHandler(IMongoDatabase database)
        {
            _context = database.GetCollection<EventLog>("eventLogs");
        }

        public async Task AddEventAsync(EventLog log)
        {
            await _context.InsertOneAsync(log);
           
        }

        public async Task<EventLog?> GetEventFromTypeByMessageId(string type, Guid id)
        {
            return await _context.Find(x => x.EventType == type && x.MessageId == id).FirstOrDefaultAsync();
        }
    }
}
