namespace PatientRegisterService.Events
{
    public record PatientRegister(string FirstName, string LastName, string PESEL, string PhoneNumber, string? Email, bool isAccountRegistred);
}
