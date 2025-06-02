using DocumentService.Application.Utils;
using DocumentService.Domain.Appointments;
using DocumentService.Domain.Documents;
using MassTransit;

namespace DocumentService.Application.Documents.Commands.UploadDocument
{
    public class UploadDocumentCommandHandler : IConsumer<UploadDocumentCommand>
    {
        private readonly IDocumentRepository _repository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IFileStorage _storage;
        private readonly ITopicProducer<DocumentCreated> _topicProducer;

        public UploadDocumentCommandHandler(IDocumentRepository repository, IFileStorage storage, IAppointmentRepository appointmentsRepository, ITopicProducer<DocumentCreated> topicProducer)
        {
            _repository = repository;
            _storage = storage;
            _appointmentRepository = appointmentsRepository;
            _topicProducer = topicProducer;
        }

        public async Task Consume(ConsumeContext<UploadDocumentCommand> context)
        {
            var req = context.Message;
            try
            {
                var appointment = await _appointmentRepository.GetById(req.AppointmentId);
                if (appointment is null)
                {
                    await context.RespondAsync(Result.Failure(new Error("404", "Not found")));
                    return;
                }
                var document = new Document(req.AppointmentId, req.Name, DateTime.UtcNow);
                await _storage.UploadFile(req.File, $"{req.AppointmentId}/${document.Id}");
                await _repository.AddDocument(document);
                var url = await _storage.GetFile($"{req.AppointmentId}/${document.Id}");
                await _topicProducer.Produce(
                    new DocumentCreated(
                        appointment!.Id,
                        url,
                        appointment.AppointmentType,
                        appointment.PatientEmail,
                        appointment.DoctorEmail,
                        appointment.Price
                    )
                );
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result.Failure(new Error("", ex.Message)));
            }
        }
    }
}
