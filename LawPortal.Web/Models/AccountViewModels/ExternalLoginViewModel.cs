using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "The Email address is not valid.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string LoginProvider { get; set; }

        public bool IsTrue => true;
        [Display(Name = "I have read and agree to the Terms of Service and Privacy Policy")]
        [Compare(nameof(IsTrue), ErrorMessage = "Please check this box to continue.")]
        public bool UserConsent { get; set; }
    }
}
