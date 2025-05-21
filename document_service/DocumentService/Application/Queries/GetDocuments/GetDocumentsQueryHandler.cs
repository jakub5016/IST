using DocumentService.Application.Utils;
using DocumentService.Domain;
using MassTransit;

namespace DocumentService.Application.Queries.GetDocuments
{
    public class GetDocumentsQueryHandler : IConsumer<GetDocumentsQuery>
    {
        private readonly IDocumentRepository _repository;

        public GetDocumentsQueryHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<GetDocumentsQuery> context)
        {
            var req = context.Message;
            try
            {
                var documents = await _repository.GetDocumentsAsync(req.AppointmentId);
                if (documents == null) {
                    await context.RespondAsync(Result.Failure(new Error("404", "Not found")));
                    return;
                }
                var response = new GetDocumentsResponse([.. documents.Select(x => new GetDocumentResponse(x.AppointmentId, x.Name, x.CreatedDate))]);
                await context.RespondAsync(Result<GetDocumentsResponse>.Success(response));
            }
            catch (Exception ex) {

                await context.RespondAsync(Result.Failure(new Error("500", ex.Message)));
            }
        }
    }
}
