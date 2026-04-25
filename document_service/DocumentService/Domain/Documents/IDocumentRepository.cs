namespace DocumentService.Domain.Documents
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetDocuments(Guid appointmentId);
        Task AddDocument(Document document);
        Task<Document?> GetDocumentById (Guid id);
    }
}
