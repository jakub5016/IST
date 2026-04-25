using DocumentService.Application.Utils;
using DocumentService.Domain.Documents;
using MassTransit;

namespace DocumentService.Application.Documents.Queries.GetDocument
{
    public class GetDocumentQueryHandler : IConsumer<GetDocumentQuery>
    {
        private readonly IDocumentRepository _repository;
        private readonly IFileStorage _storage;

        public GetDocumentQueryHandler(IDocumentRepository repository, IFileStorage storage)
        {
            _repository = repository;
            _storage = storage;
        }

        public async Task Consume(ConsumeContext<GetDocumentQuery> context)
        {
            var req = context.Message;
            try
            {
                var document = await _repository.GetDocumentById(req.Id);
                if (document is null)
                {
                    await context.RespondAsync(Result<GetDocumentUrlResponse>.Failure(new Error("404", "Not found")));
                    return;
                }
                var url = await _storage.GetFile($"{document.AppointmentId}/${req.Id}");
                var response = new GetDocumentUrlResponse(url);
                await context.RespondAsync(Result<GetDocumentUrlResponse>.Success(response));
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result<GetDocumentUrlResponse>.Failure(new Error("", ex.Message)));
            }
        }
    }
}
