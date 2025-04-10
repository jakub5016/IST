namespace PatientService.Application.Queries.GetById
{
    public record PatientResponse(string FirstName, string LastName, string PESEL, string PhoneNumber);
}
