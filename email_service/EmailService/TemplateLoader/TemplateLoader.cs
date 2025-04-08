namespace EmailService.TemplateLoader
{
    public class TemplateLoader: ITemplateLoader
    {
        public string LoadEmailContentFromTemplate(string templateName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string _emailTemplatePath = Path.Combine(currentDirectory, "Templates", $"{templateName}.html");
            string content = File.ReadAllText(_emailTemplatePath);
            return content;
        }
    }
}
