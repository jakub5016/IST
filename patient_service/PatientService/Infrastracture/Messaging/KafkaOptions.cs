namespace PatientService.Infrastracture.Messaging
{

        public class KafkaOptions
        {
            public const string KAFKA = "Kafka";

            public string ServerAddress { get; set; } = string.Empty;
            public string CreateUserTopic { get; set; } = string.Empty;
            public string UserCreatedTopic {  get; set; } = string.Empty;
            public string PatientRegistredTopic {  get; set; } = string.Empty;
            public string RegisterPatientTopic { get; set; } = string.Empty;
            public string WelcomeEmailSentTopic { get;set; } = string.Empty;
            public string SendWelcomeEmailTopic { get; set; } = string.Empty;
            public string CancelPatientRegistrationTopic { get; set; } = string.Empty;
            public string PatientRegistrationCancelledTopic {  get; set; } = string.Empty;  

    }

}
