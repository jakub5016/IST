using EmailService.EmailSender;
using EmailService.Events;
using EmailService.TemplateLoader;
using MassTransit;

namespace EmailService.Consumers
{
    public class SendWelcomeEmailCommandHandler : IConsumer<UserRegistredEvent>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;

        public SendWelcomeEmailCommandHandler(IEmailSender emailSender, ITemplateLoader templateLoader)
        {
            _emailSender = emailSender;
            _templateLoader = templateLoader;
        }

        public async Task Consume(ConsumeContext<UserRegistredEvent> context)
        {
            try
            {
                var content = _templateLoader.LoadEmailContentFromTemplate("WelcomeEmail");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(context.Message.Email, emailContent);
            }
            catch (Exception ex )
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
