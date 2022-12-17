using ASPSMS;
using MailKit.Net.Smtp;
using Mezhintekhkom.Site.Properties;
using Microsoft.Extensions.Options;
using MimeKit;
using static Mezhintekhkom.Site.Properties.MessageServiceConfiguration;

namespace Mezhintekhkom.Site.Services
{
    public class MessageService : IEmailSender, ISmsSender
    {
        private readonly IOptions<MessageServiceConfiguration> _options;
        public MessageService(IOptions<MessageServiceConfiguration> options)
        {
            _options = options;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            EmailOptions options = _options.Value.EmailSender;
            MimeMessage mail = new MimeMessage();
            MailboxAddress from = new MailboxAddress(options.From, options.Email);
            mail.From.Add(from);
            MailboxAddress to = new MailboxAddress(options.To, email);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (SmtpClient smtp = new SmtpClient())
            {
                await smtp.ConnectAsync(options.Host, options.Port, options.UseSsl);
                await smtp.AuthenticateAsync(options.Email, options.Password);
                await smtp.SendAsync(mail);
                await smtp.DisconnectAsync(true);
            }
        }

        public string GetProvider() => _options.Value.EmailSender.Provider;

        public string GetTemplateSubject(MessageType type)
        {
            string[] subjects = _options.Value.EmailSender.Subjects;
            int index = (int)type;
            string message = subjects[index];
            return message;
        }

        public string GetTemplateBody(MessageType type, params object[] values)
        {
            string[] bodies = _options.Value.EmailSender.Bodies;
            int index = (int)type;
            string message = bodies[index];
            return String.Format(message, values);
        }

        public Task SendSmsAsync(string number, string message)
        {
            SmsOptions options = _options.Value.SmsSender;
            SMS aspsms = new SMS();
            aspsms.Userkey = options.UserKey;
            aspsms.Password = options.ApiPassword;
            aspsms.Originator = options.Originator;
            aspsms.AddRecipient(number);
            aspsms.MessageData = message;
            return aspsms.SendTextSMS();
        }
    }
}
