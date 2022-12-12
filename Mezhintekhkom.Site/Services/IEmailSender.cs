namespace Mezhintekhkom.Site.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
        string GetProvider();
        string GetTemplateSubject(MessageType type);
        string GetTemplateBody(MessageType type, params object[] values);
    }
}
