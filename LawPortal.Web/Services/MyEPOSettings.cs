namespace LawPortal.Web.Services
{
    public class EPOMailboxSettings
    {
        public bool IsAPIOn { get; set; }
    }

    public class EPOOPSSettings
    {
        public string? ConsumerKey { get; set; }
        public string? ConsumerSecretKey { get; set; }
    }
}
