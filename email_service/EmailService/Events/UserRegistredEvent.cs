using EmailService.Models;

namespace EmailService.Events
{
    public record UserRegistredEvent(string Username, string ActivationLink, string Email)
    {
        public EmailMessage GetEmailMessage(string content)
        {
            content = content.Replace("{{username}}", Username)
                .Replace("{{activationLink}}", ActivationLink);
            return new EmailMessage()
            {
                Content = content,
                Subject = "Welcome to AngioCard"
            };
        }
    }
}
