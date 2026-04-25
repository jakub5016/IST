namespace DocumentService.Application.Documents.Commands.UploadDocument
{
    public record UploadDocumentCommand(Stream File, Guid AppointmentId, string Name);
}
