namespace EmailService.Infrastracture.Email.TemplateLoader
{
    public interface ITemplateLoader
    {
        public string LoadEmailContentFromTemplate(string templateName);
    }
}
