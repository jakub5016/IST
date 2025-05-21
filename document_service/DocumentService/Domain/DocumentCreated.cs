namespace DocumentService.Domain
{
    public record DocumentCreated
    {
        public string Url { get; set; }
        public string PatientEmail { get; set; }
       
    }
}
