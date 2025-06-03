using EmailService.Domain;
using EmailService.Infrastracture.Notification.Configuration;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailService.Infrastracture.Email.EmailSender
{
    public class EmailSender(IOptions<SMTPOptions> options) : IEmailSender
    {
        private readonly SMTPOptions _options = options.Value;

        public async Task SendEmailAsync(string to, EmailMessage emailContent, string? bcc = null)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.From, _options.Email));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = emailContent.Subject;
            if (bcc is not null) { message.Cc.Add(InternetAddress.Parse(bcc)); }
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = emailContent.Content
            };
            message.Body = bodyBuilder.ToMessageBody();
            using var client = new SmtpClient();
            client.Connect(_options.ServerAddress, _options.Port, false);
            client.Authenticate(_options.Email, _options.Password);
            await client.SendAsync(message);
            client.Disconnect(true);
        }
    }
}
