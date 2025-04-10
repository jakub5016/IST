namespace PatientService.Application.Command.Update
{
    public record UpdateCommand
    (
         Guid Id,
         string FirstName,
         string LastName,
         string PhoneNumber
    );
}
