using EmailService.Application.Events;
using EmailService.Domain;
using EmailService.Infrastracture.Email.EmailSender;
using EmailService.Infrastracture.Email.TemplateLoader;
using EmailService.Infrastracture.Messaging.Configuration;
using MassTransit;
using Microsoft.Extensions.Options;

namespace EmailService.Application.Commands
{

    public class SendZoomMeetingEmailCommandHandler : IConsumer<ZoomCreated>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendZoomMeetingEmailCommandHandler> _logger;
        private readonly IEventStoreHandler _eventStoreHandler;
        private readonly KafkaOptions _kafkaOptions;
        public SendZoomMeetingEmailCommandHandler(ITemplateLoader templateLoader, IEmailSender emailSender, ILogger<SendZoomMeetingEmailCommandHandler> logger, IEventStoreHandler eventStoreHandler, IOptions<KafkaOptions> kafkaOptions)
        {
            _templateLoader = templateLoader;
            _emailSender = emailSender;
            _logger = logger;
            _eventStoreHandler = eventStoreHandler;
            _kafkaOptions = kafkaOptions.Value;
        }

        public async Task Consume(ConsumeContext<ZoomCreated> context)
        {
            var req = context.Message;
            try
            {
                var oldEvent = await _eventStoreHandler.GetEventFromTypeByMessageId
                (
                    _kafkaOptions.ZoomCreatedTopic,
                    req.AppointmentId
                );
                if (oldEvent != null)
                {
                    _logger.LogWarning("Mail was already sent");
                    return;
                }

                var content = _templateLoader.LoadEmailContentFromTemplate("MeetingScheduled");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.PatientEmail, emailContent, req.DoctorEmail);

                var newEvent = new EventLog(req.AppointmentId, _kafkaOptions.ZoomCreatedTopic, req);
                await _eventStoreHandler.AddEventAsync(newEvent);

                _logger.LogInformation($"Zoom meeting email sent to: {req.PatientEmail}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
