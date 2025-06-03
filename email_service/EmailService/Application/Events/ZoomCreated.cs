using EmailService.Domain;
using MongoDB.Bson.Serialization.Attributes;

namespace EmailService.Application.Events
{
    public class ZoomCreated
    {
        public ZoomCreated()
        {
        }

        public ZoomCreated(string meetingId, Guid appointmentId, string joinUrl, string doctorEmail, string patientEmail, string appointmentType, DateTime startTime, DateTime endTime)
        {
            MeetingId = meetingId;
            AppointmentId = appointmentId;
            JoinUrl = joinUrl;
            DoctorEmail = doctorEmail;
            PatientEmail = patientEmail;
            AppointmentType = appointmentType;
            StartTime = startTime;
            EndTime = endTime;
        }

        public string MeetingId { get; set; }
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.Standard)]

        public Guid AppointmentId { get; set; }
        public string JoinUrl { get; set; }
        public string DoctorEmail { get; set; }
        public string PatientEmail { get; set; }
        public string AppointmentType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EmailMessage GetEmailMessage(string content)
        {
            content = content.Replace("{{zoomLink}}", JoinUrl)
                          .Replace("{{appointmentType}}", AppointmentType)
                          .Replace("{{startTime}}", StartTime.ToString("dd-MM-yyyy HH:mm"))
                          .Replace("{{endTime}}", EndTime.ToString("dd-MM-yyyy HH:mm"));

            return new EmailMessage()
            {
                Content = content,
                Subject = $"AngioCard: Zoom meeting scheduled"
            };
        }
    }
}
