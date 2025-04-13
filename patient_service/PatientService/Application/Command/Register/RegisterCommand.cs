namespace PatientService.Application.Command.Register
{
    public record RegisterCommand(string FirstName, string LastName, string PESEL, string PhoneNumber);
}
