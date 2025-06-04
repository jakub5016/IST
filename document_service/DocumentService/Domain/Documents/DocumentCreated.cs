namespace DocumentService.Domain.Documents
{
    public record DocumentCreated(
        Guid AppointmentId,
        string Url,
        string AppointmentType,
        DateTime StartTime,
        DateTime EndTime,
        string PatientEmail,
        string DoctorEmail,
        int Price
    );
}
