using EmailService.Models;

namespace EmailService.Events
{
    public record ZoomCreated(string MeetingId, Guid AppointmentId, string JoinUrl, string DoctorEmail, string PatientEmail, string AppointmentType, DateTime StartTime, DateTime EndTime)
    {
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
