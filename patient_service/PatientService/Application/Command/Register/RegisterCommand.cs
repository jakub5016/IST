namespace PatientService.Application.Command.Register
{
    public class RegisterCommand
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PESEL { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
