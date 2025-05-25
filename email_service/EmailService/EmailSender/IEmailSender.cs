using EmailService.Models;

namespace EmailService.EmailSender
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, EmailMessage message, string? bcc = null);
    }
}
