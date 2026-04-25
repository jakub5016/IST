namespace DocumentService.Application.Documents.Queries.GetDocuments
{
    public record GetDocumentResponse(Guid Id, string Name, DateTime CreatedTime);
}