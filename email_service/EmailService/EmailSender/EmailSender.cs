using EmailService.Configuration;
using EmailService.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailService.EmailSender
{
    public class EmailSender(IOptions<SMTPOptions> options) : IEmailSender
    {
        private readonly SMTPOptions _options = options.Value;

        public async Task SendEmailAsync(string to, EmailMessage emailContent)
        {
            var message = new MimeMessage();
            
            message.From.Add(new MailboxAddress(_options.From ,_options.Email));
            message.To.Add(new MailboxAddress(to,to));
            message.Subject = emailContent.Subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = emailContent.Content }; 
 
            using var client = new SmtpClient();
            client.Connect(_options.ServerAddress, _options.Port, false);
            client.Authenticate(_options.Email, _options.Password);
            await client.SendAsync(message);
            client.Disconnect(true);
        }
    }
}
