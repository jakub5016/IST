namespace DocumentService.Domain.Documents
{
    public record DocumentCreated(
        Guid AppointmentId,
        string Url,
        string AppointmentType,
        string PatientEmail,
        string DoctorEmail,
        int Price
    );
}
