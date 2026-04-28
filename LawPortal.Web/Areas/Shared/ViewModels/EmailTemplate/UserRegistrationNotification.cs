using LawPortal.Core.Identity;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class UserRegistrationNotification : EmailContent
    {
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }
        public string? UserEmail { get; set; }
        public CPiUserType UserType { get; set; }
        public CPiUserStatus UserStatus { get; set; }
        public string? CallToAction { get; set; }
        public string? CallToActionUrl { get; set; }
    }
}
