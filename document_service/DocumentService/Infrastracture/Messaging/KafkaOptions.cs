namespace DocumentService.Infrastracture.Messaging
{

    public class KafkaOptions
    {
        public const string KAFKA = "Kafka";
    
        public string ServerAddress { get; set; } = string.Empty;
        public string DocumentCreatedTopic {  get; set; } = string.Empty;
        public string FinishAppointmentTopic {  get; set; } = string.Empty;
    }

}
