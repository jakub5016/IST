using EmailService.EmailSender;
using EmailService.Events;
using EmailService.TemplateLoader;
using MassTransit;

namespace EmailService.Commands
{

    public class SendZoomMeetingEmailCommandHandler : IConsumer<ZoomCreated>
    {
        private readonly ITemplateLoader _templateLoader;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendZoomMeetingEmailCommandHandler> _logger;

        public SendZoomMeetingEmailCommandHandler(ITemplateLoader templateLoader, IEmailSender emailSender, ILogger<SendZoomMeetingEmailCommandHandler> logger)
        {
            _templateLoader = templateLoader;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ZoomCreated> context)
        {
            var req = context.Message;
            try
            {
                var content = _templateLoader.LoadEmailContentFromTemplate("MeetingScheduled");
                var emailContent = context.Message.GetEmailMessage(content);
                await _emailSender.SendEmailAsync(req.PatientEmail, emailContent, req.DoctorEmail);
                _logger.LogInformation($"Welcome email sent to: {req.PatientEmail}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
