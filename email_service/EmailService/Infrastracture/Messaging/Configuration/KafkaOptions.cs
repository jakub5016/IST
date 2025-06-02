namespace EmailService.Infrastracture.Messaging.Configuration
{
    public class KafkaOptions
    {
        public const string KAFKA = "Kafka";

        public string ServerAddress { get; set; } = string.Empty;
        public string UserRegistredTopic {  get; set; } = string.Empty;
        public string ChangePasswordTopic { get; set; } = string.Empty;
        public string AppointmentCreatedTopic {  get; set; } = string.Empty;
        public string AppointmentCancelledTopic { get; set; } = string.Empty;
        public string ZoomCreatedTopic {  get; set; } = string.Empty;

    }
}
