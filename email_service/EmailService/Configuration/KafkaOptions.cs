namespace EmailService.Configuration
{
    public class KafkaOptions
    {
        public const string KAFKA = "Kafka";

        public string ServerAddress { get; set; } = string.Empty;
        public string UserRegistredTopic {  get; set; } = string.Empty;

    }
}
