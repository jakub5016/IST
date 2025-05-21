namespace DocumentService.Application.Queries.GetDocuments
{
    public record GetDocumentResponse(Guid Id, string Name, DateTime CreatedTime);
}