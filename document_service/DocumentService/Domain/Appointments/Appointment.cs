namespace DocumentService.Domain.Appointments
{
    public record Appointment(
        Guid Id,
        DateTime StartTime,
        DateTime EndTime,
        string AppointmentType,
        string PatientEmail,
        string DoctorEmail,
        int Price
    );
   
}
