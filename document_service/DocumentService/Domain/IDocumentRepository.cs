namespace DocumentService.Domain
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetDocumentsAsync(Guid appointmentId);
        Task AddDocument(Document document);
        Task<Document?> GetDocumentById (Guid id);
    }
}
