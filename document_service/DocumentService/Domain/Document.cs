namespace DocumentService.Domain
{
    public class Document(Guid appointmentId, string name, DateTime createdDate)
    {
        public Guid Id { get; set; } =  Guid.NewGuid();
        public Guid AppointmentId { get; set; } = appointmentId;
        public string Name { get; set; } = name;
        public DateTime CreatedDate { get; set; } = createdDate;
    }
}
