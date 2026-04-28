using LawPortal.Core.Identity;
using System.ComponentModel.DataAnnotations;

namespace LawPortal.Web.Areas.Admin.ViewModels
{
    public class UserListViewModel
    {
        public int PkId { get; set; }

        public string? Id { get; set; }
        public string? Email { get; set; }

        [Display(Name = "Name")]
        public string? FullName { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Status")]
        public CPiUserStatus Status { get; set; }

        [Display(Name = "User Type")]
        public CPiUserType UserType { get; set; }

        [Display(Name = "Last Login")]
        public DateTime? LastLoginDate { get; set; }

        public string? StatusDisplay { get; set; }
        public string? UserTypeDisplay { get; set; }
        public bool IsSelf {get;set;}

        public bool IsSuper { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsPending { get; set; }
        public bool IsLockedOut { get; set; }
        public bool Inactive { get; set; }
    }
}
