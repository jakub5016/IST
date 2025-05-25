using EmailService.EmailSender;
using EmailService.Events;
using EmailService.TemplateLoader;
using MassTransit;

namespace EmailService.Commands
{
    public class SendNewAppointmentEmailCommandHandler : IConsumer<AppointmentCreated>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendNewAppointmentEmailCommandHandler> _logger;
        public SendNewAppointmentEmailCommandHandler(ITemplateLoader templateLoader, IEmailSender emailSender, ILogger<SendNewAppointmentEmailCommandHandler> logger)
        {
            _templateLoader = templateLoader;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AppointmentCreated> context)
        {
            var req = context.Message;
            try
            {
                var content = _templateLoader.LoadEmailContentFromTemplate("NewAppointment");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.PatientEmail, emailContent, req.DoctorEmail);
                _logger.LogInformation($"Create appointment email sent to: {req.PatientEmail}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
