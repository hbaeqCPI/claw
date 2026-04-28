namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class UserAccountEmail : EmailContent
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? CallToAction { get; set; }
        public string? CallToActionUrl { get; set; }
    }
}
