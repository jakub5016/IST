namespace PatientService.Domain
{
    public interface IUnitOfWork
    {
        Task SaveChangesAsync();
    }
}
