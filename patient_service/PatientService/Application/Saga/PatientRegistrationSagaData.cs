using MassTransit;

namespace PatientService.Application.Saga
{
    public class PatientRegistrationSagaData : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public Guid PatientId { get; set; }
        public string Email { get; set; }
    }
}
