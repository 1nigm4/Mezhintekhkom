namespace Mezhintekhkom.Site.Properties
{
    public class MessageServiceConfiguration
    {
        public EmailOptions EmailSender { get; set; }
        public SmsOptions SmsSender { get; set; }

        public class EmailOptions
        {
            public string Provider { get; set; }
            public string Host { get; set; }
            public ushort Port { get; set; }
            public bool UseSsl { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string From { get; set; }
            public string To { get; set; }
            public string[] Subjects { get; set; }
            public string[] Bodies { get; set; }
        }

        public class SmsOptions
        {
            public string Provider { get; set; }
            public string UserKey { get; set; }
            public string ApiPassword { get; set; }
            public string Originator { get; set; }
        }
    }
}
