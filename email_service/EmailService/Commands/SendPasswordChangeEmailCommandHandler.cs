using EmailService.EmailSender;
using EmailService.Events;
using EmailService.TemplateLoader;
using MassTransit;

namespace EmailService.Commands
{
    public class SendPasswordChangeEmailCommandHandler : IConsumer<ChangePassword>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendPasswordChangeEmailCommandHandler> _logger;
        public SendPasswordChangeEmailCommandHandler(ITemplateLoader templateLoader, IEmailSender emailSender, ILogger<SendPasswordChangeEmailCommandHandler> logger)
        {
            _templateLoader = templateLoader;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ChangePassword> context)
        {
            var req = context.Message;
            try
            {
                var content = _templateLoader.LoadEmailContentFromTemplate("PasswordChange");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.Email, emailContent);
                _logger.LogInformation($"Change password email sent to: {req.Email}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
