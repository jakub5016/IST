using DocumentService.Application.Utils;
using DocumentService.Domain;
using MassTransit;

namespace DocumentService.Application.Commands.UploadDocument
{
    public class UploadDocumentCommandHandler : IConsumer<UploadDocumentCommand>
    {
        private readonly IDocumentRepository _repository;
        private readonly IFileStorage _storage;

        public UploadDocumentCommandHandler(IDocumentRepository repository, IFileStorage storage)
        {
            _repository = repository;
            _storage = storage;
        }

        public async Task Consume(ConsumeContext<UploadDocumentCommand> context)
        {
            var req = context.Message;
            try
            {
                var document = new Document(req.AppointmentId, req.Name, DateTime.UtcNow);
                await _storage.UploadFile(req.File, $"{req.AppointmentId}/${document.Id}");
                await _repository.AddDocument(document);
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex) {
                await context.RespondAsync(Result.Failure(new Error("",ex.Message)));

            }
        }
    }
}
