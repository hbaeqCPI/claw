
namespace R10.Web.Areas.Shared.ViewModels
{
    public class FFReminderCoverLetter : EmailContent
    {
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? ClientCode { get; set; }
        public string? ClientName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactGreeting { get; set; }
        public string? RecipientList { get; set; }
        public string? CallToAction { get; set; }
        public string? CallToActionUrl { get; set; }
    }
}
