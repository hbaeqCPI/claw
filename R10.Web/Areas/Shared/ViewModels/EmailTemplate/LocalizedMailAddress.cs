using System.Net.Mail;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class LocalizedMailAddress : MailAddress
    {
        public string? Locale { get; }
        public MailAddress? MailAddress { get; }
        public LocalizedMailAddress(string address, string displayName, string locale) : base(address, displayName)
        {
            Locale = locale;
            MailAddress = this;
        }
    }
}
