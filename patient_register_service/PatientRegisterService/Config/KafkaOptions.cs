namespace PatientRegisterService.Config
{

        public class KafkaOptions
        {
            public const string KAFKA = "Kafka";

            public string ServerAddress { get; set; } = string.Empty;
            public string PatientRegisterTopic {  get; set; } = string.Empty;
    }

}
