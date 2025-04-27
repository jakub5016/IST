
namespace PatientService.Application.Saga
{
    public record CreateUser(Guid PatientId, string Email);
}