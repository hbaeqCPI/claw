using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required]
        //[EmailAddress(ErrorMessage = "The Email address is not valid.")]
        [Display(Name = "User Id")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Keep me signed in")]
        public bool StaySignedIn { get; set; }

        [Display(Name = "Remember me")]
        public bool SaveEmail { get; set; }

        public SystemStatusType SystemStatusType { get; set; }

        public SystemNotification? CookieConsent { get; set; }
    }
}
