using EmailService.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace EmailService.Application.Events
{
    public class ChangePassword
    {
        public ChangePassword()
        {
        }

        public ChangePassword(Guid id, Guid patientId, string username, string url, string code, string email)
        {
            Id = id;
            PatientId = patientId;
            Username = username;
            Url = url;
            Code = code;
            Email = email;
        }
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]

        public Guid Id { get; set; }
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]

        public Guid PatientId { get; set; }
        public string Username { get; set; }
        public string Url { get; set; }
        public string Code { get; set; }
        public string Email {  get; set; }
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
