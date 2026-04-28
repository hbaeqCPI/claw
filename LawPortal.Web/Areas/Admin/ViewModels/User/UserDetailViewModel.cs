using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Admin.ViewModels
{
    public class UserDetailViewModel
    {
        public int PkId { get; set; }
        public string? Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(256)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        //[StringLength(100, ErrorMessage = "The password must be at least {0} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Email new password")]
        public bool EmailNewPassword { get; set; }

        [Display(Name = "Require user to change password")]
        public bool RequireChangePassword { get; set; }

        [StringLength(256)]
        public string Locale { get; set; } = "en-US";

        [Required]
        [Display(Name = "Status")]
        //.net 6.0 model binding issue with enum???
        //public CPiUserStatus Status { get; set; }
        public int Status { get; set; }

        [Required]
        [Display(Name = "User Type")]
        //.net 6.0 model binding issue with enum???
        //public CPiUserType UserType { get; set; }
        public int UserType { get; set; }

        public int EntityFilterType { get; set; }

        public string? FullName { get; set; }
        public string? Initials { get; set; }
        public string? StatusDisplay { get; set; }
        public string? UserTypeDisplay { get; set; }

        [Display(Name = "Last Login Date")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Last Password Change Date")]
        public DateTime? LastPasswordChangeDate { get; set; }

        [Display(Name = "Lock Out End")]
        public DateTime? LockoutEnd { get; set; }

        public bool IsEnabled { get; set; }
        public bool IsPending { get; set; }
        public bool IsLockedOut { get; set; }
        public string? PasswordHash { get; set; }
        public bool RequiresConfirmedEmail { get; set; }
        public bool Inactive { get; set; }

        public string? CopyId { get; set; }
        public int? EntityId { get; set; }

        public string? StatusMessage { get; set; }

        [Display(Name = "Password never expires")]
        public bool PasswordNeverExpires { get; set; }

        [Display(Name = "Effective Date")]
        public DateTime? ValidDateFrom { get; set; }

        [Display(Name = "End Date")]
        public DateTime? ValidDateTo { get; set; }

        [Display(Name = "User cannot change password")]
        public bool CannotChangePassword { get; set; }


        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }

        public byte[]? tStamp { get; set; }

        [Display(Name = "User uses CPI Outlook Add-in")]
        public bool UseOutlookAddIn { get; set; }
        [Display(Name = "Hourly Rate")]
        public decimal? HourlyRate { get; set; }

        [Display(Name = "Web API Access Only")]
        public bool WebApiAccessOnly { get; set; }

        [Display(Name = "User must login using SSO")]
        public bool ExternalLoginOnly { get; set; }
    }
}
