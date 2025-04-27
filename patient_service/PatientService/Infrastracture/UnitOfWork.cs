using PatientService.Domain.Shared;
using PatientService.Infrastracture.Database;

namespace PatientService.Infrastracture
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PatientContext _context;

        public UnitOfWork(PatientContext context)
        {
            _context = context;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        //ToDo: add transaction methods
    }
}
