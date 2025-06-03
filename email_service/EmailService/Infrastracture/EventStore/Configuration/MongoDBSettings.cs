namespace EmailService.Infrastracture.EventStore.Configuration
{
    public class MongoDBSettings
    {
        public const string MONGO = "Database"; 
        public string ConnectionString {  get; set; }= string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}
