namespace DocumentService.Domain.Appointments
{
    public interface IAppointmentRepository
    {
        Task Add(Appointment appointment);
        Task<Appointment?> GetById(Guid id);
    }
}
