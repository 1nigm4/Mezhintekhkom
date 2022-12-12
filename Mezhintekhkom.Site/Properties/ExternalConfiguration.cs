namespace Mezhintekhkom.Site.Properties
{
    public class ExternalConfiguration
    {
        public OAuthConfiguration Vkontakte { get; set; }
        public OAuthConfiguration Google { get; set; }
        public OAuthConfiguration Yandex { get; set; }

        public class OAuthConfiguration
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string[] Scopes { get; set; }
            public string[] Fields { get; set; }
        }
    }
}
