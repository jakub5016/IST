using EmailService.EmailSender;
using EmailService.Events;
using EmailService.TemplateLoader;
using MassTransit;

namespace EmailService.Commands
{
    public class SendCancelAppointmentEmailCommandHandler: IConsumer<AppointmentCancelled>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendCancelAppointmentEmailCommandHandler> _logger;

        public SendCancelAppointmentEmailCommandHandler(ITemplateLoader templateLoader, IEmailSender emailSender, ILogger<SendCancelAppointmentEmailCommandHandler> logger)
        {
            _templateLoader = templateLoader;
            _emailSender = emailSender;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<AppointmentCancelled> context)
        {
            var req = context.Message;
            try
            {
                var content = _templateLoader.LoadEmailContentFromTemplate("CancelAppointment");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.PatientEmail, emailContent, req.DoctorEmail);
                _logger.LogInformation($"Appointment cancelled email sent to: {req.PatientEmail}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

    }

}
