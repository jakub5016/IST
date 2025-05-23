using EmailService.Models;

namespace EmailService.Events
{
    public record AppointmentCreated(Guid AppointmentId, Guid PatientId, string AppointmentType, string Username, string PatientEmail, string DoctorEmail, float Price, DateTime StartTime, DateTime EndTime)
    {
        public EmailMessage GetEmailMessage(string content)
        {
            content = content.Replace("{{username}}", Username)
                .Replace("{{price}}", Price.ToString())
                .Replace("{{username}}", Username)
                .Replace("{{appointmentType}}", AppointmentType)
                .Replace("{{startTime}}", StartTime.ToString("dd-MM-yyyy HH:mm"))
                .Replace("{{endTime}}", EndTime.ToString("dd-MM-yyyy HH:mm"));

            return new EmailMessage()
            {
                Content = content,
                Subject = $"AngioCard: Appointment {AppointmentId} scheduled"
            };
        }
    }
}
