namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class UserAccountApprovalNotification : EmailContent
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? CallToAction { get; set; }
        public string? CallToActionUrl { get; set; }
    }
}
