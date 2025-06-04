using DocumentService.Domain.Documents;
using EmailService.Domain;
using EmailService.Infrastracture.Email.EmailSender;
using EmailService.Infrastracture.Email.TemplateLoader;
using EmailService.Infrastracture.Messaging.Configuration;
using MassTransit;
using Microsoft.Extensions.Options;

namespace EmailService.Application.Commands
{
    public class SendDocumentEmailCommandHandler : IConsumer<DocumentCreated>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendDocumentEmailCommandHandler> _logger;
        private readonly IEventStoreHandler _eventStoreHandler;
        private readonly KafkaOptions _kafkaOptions;

        public SendDocumentEmailCommandHandler(ITemplateLoader templateLoader, IEmailSender emailSender, ILogger<SendDocumentEmailCommandHandler> logger, IEventStoreHandler eventStoreHandler, IOptions<KafkaOptions> kafkaOptions)
        {
            _templateLoader = templateLoader;
            _emailSender = emailSender;
            _logger = logger;
            _eventStoreHandler = eventStoreHandler;
            _kafkaOptions = kafkaOptions.Value;
        }

        public async Task Consume(ConsumeContext<DocumentCreated> context)
        {
            var req = context.Message;
            try
            {

                var latestEvent = await _eventStoreHandler.GetEventFromTypeByMessageId
                (
                    _kafkaOptions.DocumentCreatedTopic,
                    req.AppointmentId
                );
                if (latestEvent != null)
                {
                    _logger.LogWarning("Mail was already sent");
                    return;
                }

                var content = _templateLoader.LoadEmailContentFromTemplate("SendDoctorNotes");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.PatientEmail, emailContent, req.DoctorEmail);

                var newEvent = new EventLog(req.AppointmentId, _kafkaOptions.DocumentCreatedTopic, req);
                await _eventStoreHandler.AddEventAsync(newEvent);

                _logger.LogInformation($"Create document email sent to: {req.PatientEmail}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
