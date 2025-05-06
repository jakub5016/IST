namespace EmployeeService.Infrastracture.Messaging
{
    public class KafkaOptions
    {
        public const string KAFKA = "Kafka";

        public string ServerAddress { get; set; } = string.Empty;
        public string EmployeeHiredTopic { get; set; } = string.Empty;
        public string EmployeeRegistrationFailedTopic { get; set; } = string.Empty;

    }
}
