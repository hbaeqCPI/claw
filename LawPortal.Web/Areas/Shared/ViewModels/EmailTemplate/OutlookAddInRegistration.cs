namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class OutlookAddInRegistration : EmailContent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CallToAction { get; set; }
        public string CallToActionUrl { get; set; }
        public string ClientSecret { get; set; }
        public string ClientId { get; set; }
        public string OutlookUrl { get; set; }
        public string ClientName { get; set; }
    }
}
