using DocumentService.Domain.Appointments;
using Microsoft.EntityFrameworkCore;

namespace DocumentService.Infrastracture.Database
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly DocumentContext _context;

        public AppointmentRepository(DocumentContext context)
        {
            _context = context;
        }

        public async Task Add(Appointment appointment)
        {
            await _context.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<Appointment?> GetById(Guid id)
        {
            return await _context.Appointments.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
