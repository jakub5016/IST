using EmailService.Application.Events;
using EmailService.Domain;
using EmailService.Infrastracture.Email.EmailSender;
using EmailService.Infrastracture.Email.TemplateLoader;
using EmailService.Infrastracture.Messaging.Configuration;
using MassTransit;
using Microsoft.Extensions.Options;

namespace EmailService.Application.Commands
{
    public class SendPasswordChangeEmailCommandHandler : IConsumer<ChangePassword>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendPasswordChangeEmailCommandHandler> _logger;
        private readonly IEventStoreHandler _eventStoreHandler;
        private readonly KafkaOptions _kafkaOptions;

        public SendPasswordChangeEmailCommandHandler(ITemplateLoader templateLoader, IEmailSender emailSender, ILogger<SendPasswordChangeEmailCommandHandler> logger, IEventStoreHandler eventStoreHandler, IOptions<KafkaOptions> kafkaOptions)
        {
            _templateLoader = templateLoader;
            _emailSender = emailSender;
            _logger = logger;
            _eventStoreHandler = eventStoreHandler;
            _kafkaOptions = kafkaOptions.Value;
        }

        public async Task Consume(ConsumeContext<ChangePassword> context)
        {
            var req = context.Message;
            try
            {
                var latestEvent = await _eventStoreHandler.GetEventFromTypeByMessageId
                (
                   _kafkaOptions.ChangePasswordTopic,
                   req.Id
                );
                if (latestEvent != null)
                {
                    _logger.LogWarning("Mail was already sent");
                    return;
                }

                var content = _templateLoader.LoadEmailContentFromTemplate("PasswordChange");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.Email, emailContent);

                var newEvent = new EventLog(req.Id, _kafkaOptions.ChangePasswordTopic, req);
                await _eventStoreHandler.AddEventAsync(newEvent);

                _logger.LogInformation($"Change password email sent to: {req.Email}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
