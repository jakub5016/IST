using EmailService.Application.Events;
using EmailService.Domain;
using EmailService.Infrastracture.Email.EmailSender;
using EmailService.Infrastracture.Email.TemplateLoader;
using EmailService.Infrastracture.Messaging.Configuration;
using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace EmailService.Application.Commands
{
    public class SendWelcomeEmailCommandHandler : IConsumer<UserRegistred>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendWelcomeEmailCommandHandler> _logger;
        private readonly IEventStoreHandler _eventStoreHandler;
        private readonly KafkaOptions _kafkaOptions;

        public SendWelcomeEmailCommandHandler(IEmailSender emailSender, ITemplateLoader templateLoader, ILogger<SendWelcomeEmailCommandHandler> logger, IEventStoreHandler eventStoreHandler, IOptions<KafkaOptions> kafkaOptions)
        {
            _emailSender = emailSender;
            _templateLoader = templateLoader;
            _logger = logger;
            _eventStoreHandler = eventStoreHandler;
            _kafkaOptions = kafkaOptions.Value;
        }

        public async Task Consume(ConsumeContext<UserRegistred> context)
        {
            var req = context.Message;
            try
            {
                var latestEvent = await _eventStoreHandler.GetEventFromTypeByMessageId
                (
                    _kafkaOptions.UserRegistredTopic,
                    req.PatientId
                );
                if (latestEvent != null) {
                    _logger.LogWarning("Mail was already sent");
                    return;
                }

                var content = _templateLoader.LoadEmailContentFromTemplate("WelcomeEmail");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.Email, emailContent);

                var newEvent = new EventLog(req.PatientId, _kafkaOptions.UserRegistredTopic, req );
                await _eventStoreHandler.AddEventAsync(newEvent);

                _logger.LogInformation($"Welcome email sent to: {req.Email}");

            }
            catch (Exception ex )
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
