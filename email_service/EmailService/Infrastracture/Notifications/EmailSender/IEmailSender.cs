using EmailService.Domain;

namespace EmailService.Infrastracture.Email.EmailSender
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, EmailMessage message, string? bcc = null);
    }
}
