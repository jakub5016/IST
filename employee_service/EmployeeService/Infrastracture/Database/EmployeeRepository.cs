using EmployeeService.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Infrastracture.Database
{
    public class EmployeeRepository(EmployeeContext context) : IEmployeeRepository
    {
        private readonly EmployeeContext _context = context;

        public async Task Add(Employee entity)
        {
            await _context.Employee.AddAsync(entity);
        }

        public void Delete(Employee entity)
        {
            _context.Employee.Remove(entity);
        }

        public async Task<List<Employee>> GetAll()
        {
            return await _context.Employee.ToListAsync();
        }

        public async Task<List<Employee>> GetAllDoctors()
        {
            return await _context.Employee.Include(x => x.Doctor)
                .Where(x => x.Doctor != null)
                .ToListAsync();
        }

        public async Task<Employee?> GetById(Guid id)
        {
            return await _context.Employee.Include(x=>x.Doctor).FirstOrDefaultAsync(x => x.Id == id); // ToDo: exact without include to another method
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
