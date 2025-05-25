using EmailService.Models;

namespace EmailService.Events
{
    public record AppointmentCancelled(Guid AppointmentId, Guid PatientId, string AppointmentType, string Username, string PatientEmail, string DoctorEmail, float Price, DateTime StartTime, DateTime EndTime)
    {
        public EmailMessage GetEmailMessage(string content)
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
                Subject = $"AngioCard: Appointment cancelled"
            };
        }
    }
}

