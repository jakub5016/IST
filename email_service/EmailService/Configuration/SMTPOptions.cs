namespace EmailService.Configuration
{
    public class SMTPOptions
    {
        public const string SMTP = "SMTP";

        public string ServerAddress { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
