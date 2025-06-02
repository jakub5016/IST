using EmailService.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace EmailService.Application.Events
{
    public class AppointmentCreated
    {
        public AppointmentCreated()
        {
        }

        public AppointmentCreated(Guid appointmentId, Guid patientId, string appointmentType, string username, string patientEmail, string doctorEmail, float price, DateTime startTime, DateTime endTime)
        {
            AppointmentId = appointmentId;
            PatientId = patientId;
            AppointmentType = appointmentType;
            Username = username;
            PatientEmail = patientEmail;
            DoctorEmail = doctorEmail;
            Price = price;
            StartTime = startTime;
            EndTime = endTime;
        }
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
        public Guid AppointmentId { get; set; }
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
        public Guid PatientId { get; set; }
        public string AppointmentType { get; set; }
        public string Username { get; set; }
        public string PatientEmail { get; set; }
        public string DoctorEmail { get; set; }
        public float Price { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public  EmailMessage GetEmailMessage(string content)
        {
            content = content.Replace("{{username}}", Username)
                .Replace("{{price}}", Price.ToString())
                .Replace("{{appointmentType}}", AppointmentType)
                .Replace("{{startTime}}", StartTime.ToString("dd-MM-yyyy HH:mm"))
                .Replace("{{endTime}}", EndTime.ToString("dd-MM-yyyy HH:mm"))
                .Replace("{{appointmentId}}", AppointmentId.ToString());

            return new EmailMessage()
            {
                Content = content,
                Subject = $"AngioCard: Appointment scheduled"
            };
        }
    }
}
