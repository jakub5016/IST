using EmailService.Models;

namespace EmailService.Events
{
    public record ChangePassword(Guid Id, string Username, string Url, string Code, string Email)
    {
        public EmailMessage GetEmailMessage(string content)
        {
            content = content.Replace("{{username}}", Username)
                .Replace("{{url}}", Url)
                .Replace("{{code}}", Code);
            return new EmailMessage()
            {
                Content = content,
                Subject = "Change password"
            };
        }
    }
}
