namespace DocumentService.Application.Commands.UploadDocument
{
    public record UploadDocumentCommand(Stream File, Guid AppointmentId, string PatientEmail, string Name);
}
