using EmailService.EmailSender;
using EmailService.Events;
using EmailService.TemplateLoader;
using MassTransit;

namespace EmailService.Consumers
{
    public class SendWelcomeEmailCommandHandler : IConsumer<UserRegistred>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendWelcomeEmailCommandHandler> _logger;

        public SendWelcomeEmailCommandHandler(IEmailSender emailSender, ITemplateLoader templateLoader, ILogger<SendWelcomeEmailCommandHandler> logger)
        {
            _emailSender = emailSender;
            _templateLoader = templateLoader;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserRegistred> context)
        {
            var req = context.Message;
            try
            {
                var content = _templateLoader.LoadEmailContentFromTemplate("WelcomeEmail");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.Email, emailContent);
                _logger.LogInformation($"Welcome email sent to: {req.Email}");

            }
            catch (Exception ex )
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
