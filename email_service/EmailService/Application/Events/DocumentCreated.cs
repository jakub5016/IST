using EmailService.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentService.Domain.Documents
{
    public class DocumentCreated
    {
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]
        public Guid AppointmentId { get; set; }
        public string Url { get; set; }
        public string AppointmentType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string PatientEmail { get; set; }
        public string DoctorEmail { get; set; }
        public int Price { get; set; }

        public DocumentCreated(
            Guid appointmentId,
            string url,
            string appointmentType,
            DateTime startTime,
            DateTime endTime,
            string patientEmail,
            string doctorEmail,
            int price)
        {
            AppointmentId = appointmentId;
            Url = url;
            AppointmentType = appointmentType;
            StartTime = startTime;
            EndTime = endTime;
            PatientEmail = patientEmail;
            DoctorEmail = doctorEmail;
            Price = price;
        }

        internal EmailMessage GetEmailMessage(string content)
        {
            content = content.Replace("{{url}}", Url)
                .Replace("{{price}}", Price.ToString())
                .Replace("{{appointmentType}}", AppointmentType)
                .Replace("{{startTime}}", StartTime.ToString("dd-MM-yyyy HH:mm"))
                .Replace("{{endTime}}", EndTime.ToString("dd-MM-yyyy HH:mm"))
                .Replace("{{appointmentId}}", AppointmentId.ToString());

            return new EmailMessage()
            {
                Content = content,
                Subject = "AngioCard: Document sent"
            };
        }
    }
}
