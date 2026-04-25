using DocumentService.Application.Utils;
using DocumentService.Domain.Appointments;
using MassTransit;

namespace DocumentService.Application.Appointments.Commands.Finish
{
    public record FinishAppointmentCommand(
        Guid AppointmentId,
        string Username,
        string AppointmentType,
        DateTime StartTime,
        DateTime EndTime,
        Guid PatientId,
        string PatientEmail,
        string DoctorEmail,
        int Price
    );
    public class FinishAppointmentCommandHandler(IAppointmentRepository appointmentsRepository) : IConsumer<FinishAppointmentCommand>
    {
        private readonly IAppointmentRepository _appointmentsRepository = appointmentsRepository;

        public async Task Consume(ConsumeContext<FinishAppointmentCommand> context)
        {
            var req = context.Message;
            try
            {
                var appointment = new Appointment(
                    req.AppointmentId,
                    req.StartTime,
                    req.EndTime,
                    req.AppointmentType,
                    req.PatientEmail,
                    req.DoctorEmail,
                    req.Price
                );
                await _appointmentsRepository.Add(appointment);
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result.Failure(new Error("", ex.Message)));
            }
        }
    }
}
