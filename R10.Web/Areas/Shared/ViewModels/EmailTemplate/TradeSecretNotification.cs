namespace R10.Web.Areas.Shared.ViewModels.EmailTemplate
{
    public class TradeSecretRequestNotification : EmailContent
    {
        public string? CallToAction { get; set; }
        public string? CallToActionUrl { get; set; }
    }

    public class TradeSecretAccessCodeNofication : EmailContent
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? AccessCode { get; set; }
        public string? CallToAction { get; set; }
        public string? CallToActionUrl { get; set; }
    }
}
