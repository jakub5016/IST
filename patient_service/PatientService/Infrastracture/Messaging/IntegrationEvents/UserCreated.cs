namespace PatientService.Infrastracture.Messaging.IntegrationEvents
{
    public record UserCreated(Guid PatientId, string Email);
}
