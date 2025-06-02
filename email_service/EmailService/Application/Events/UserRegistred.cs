using EmailService.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace EmailService.Application.Events
{
    public class UserRegistred
    {
        public UserRegistred()
        {
        }

        public UserRegistred(Guid patientId, string username, string activationLink, string email)
        {
            PatientId = patientId;
            Username = username;
            ActivationLink = activationLink;
            Email = email;
        }
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
        public Guid PatientId { get; set; }
        public string Username { get; set; }
        public string ActivationLink { get; set; }
        public string Email { get; set; }
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
