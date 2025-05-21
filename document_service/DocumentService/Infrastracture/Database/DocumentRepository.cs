using DocumentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace DocumentService.Infrastracture.Database
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly DocumentContext _context;

        public DocumentRepository(DocumentContext context)
        {
            _context = context;
        }

        public async Task AddDocument(Document document)
        {
            await _context.Documents.AddAsync(document);    
            await _context.SaveChangesAsync();
        }

        public async Task<Document?> GetDocumentById(Guid id)
        {
            return await _context.Documents.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Document>> GetDocumentsAsync(Guid appointmentId)
        {
           return await _context.Documents.Where(x => x.AppointmentId == appointmentId)
                .ToListAsync();
        }
    }
}
