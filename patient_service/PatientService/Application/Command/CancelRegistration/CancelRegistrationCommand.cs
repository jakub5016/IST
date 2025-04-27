namespace PatientService.Application.Command.UndoRegistration
{
    public record CancelRegistrationCommand(Guid PatientId);
}
