using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace EmailService.Domain
{
    public class EventLog
    {
        public EventLog()
        {
        }
       
        public EventLog(Guid messageId, string eventType, object? payload)
        {
            MessageId = messageId;
            EventType = eventType;
            ReceivedAtUtc = DateTime.UtcNow; 
            if (payload != null)
            {
               
                Payload = BsonDocumentWrapper.Create(payload);
            }
            else
            {
                Payload = null!; 
            }
        }

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; } = Guid.NewGuid();
        [BsonRepresentation(BsonType.String)]
        public Guid MessageId { get; set; }
        public string EventType { get; set; }
        public BsonDocument? Payload { get; set; } 

        public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
