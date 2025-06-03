using EmailService.Application.Events;
using EmailService.Domain;
using EmailService.Infrastracture.Email.EmailSender;
using EmailService.Infrastracture.Email.TemplateLoader;
using EmailService.Infrastracture.Messaging.Configuration;
using MassTransit;
using Microsoft.Extensions.Options;

namespace EmailService.Application.Commands
{
    public class SendNewAppointmentEmailCommandHandler : IConsumer<AppointmentCreated>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendNewAppointmentEmailCommandHandler> _logger;
        private readonly IEventStoreHandler _eventStoreHandler;
        private readonly KafkaOptions _kafkaOptions;

        public SendNewAppointmentEmailCommandHandler(ITemplateLoader templateLoader, IEmailSender emailSender, ILogger<SendNewAppointmentEmailCommandHandler> logger, IEventStoreHandler eventStoreHandler, IOptions<KafkaOptions> kafkaOptions)
        {
            _templateLoader = templateLoader;
            _emailSender = emailSender;
            _logger = logger;
            _eventStoreHandler = eventStoreHandler;
            _kafkaOptions = kafkaOptions.Value;
        }

        public async Task Consume(ConsumeContext<AppointmentCreated> context)
        {
            var req = context.Message;
            try
            {
               
                var latestEvent = await _eventStoreHandler.GetEventFromTypeByMessageId
                (
                    _kafkaOptions.AppointmentCreatedTopic,
                    req.AppointmentId
                );
                if (latestEvent != null)
                {
                    _logger.LogWarning("Mail was already sent");
                    return;
                }
                var content = _templateLoader.LoadEmailContentFromTemplate("NewAppointment");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.PatientEmail, emailContent, req.DoctorEmail);

                var newEvent = new EventLog(req.AppointmentId, _kafkaOptions.AppointmentCreatedTopic, req);
                await _eventStoreHandler.AddEventAsync(newEvent);

                _logger.LogInformation($"Create appointment email sent to: {req.PatientEmail}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
