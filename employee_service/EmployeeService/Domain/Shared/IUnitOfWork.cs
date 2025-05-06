namespace PatientService.Domain.Shared
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
    }
}
