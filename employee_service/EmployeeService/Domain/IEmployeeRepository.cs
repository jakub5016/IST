namespace EmployeeService.Domain
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetById(Guid id);
        Task<List<Employee>> GetAll();
        Task<List<Employee>> GetAllDoctors();
        Task Add(Employee entity);
        void Delete(Employee entity);
        Task SaveChangesAsync();

    }
}
