namespace DocumentService.Application.Queries.GetDocuments
{
    public record GetDocumentResponse(Guid AppointmentId, string Name, DateTime CreatedTime);
}