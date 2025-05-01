namespace PatientRegisterService.Commands.Register
{
    public record RegisterCommand(string FirstName, string LastName, string PESEL, string PhoneNumber, string? Email, bool isAccountRegistred);
}
