namespace EmailService.TemplateLoader
{
    public interface ITemplateLoader
    {
        public string LoadEmailContentFromTemplate(string templateName);

    }
}
