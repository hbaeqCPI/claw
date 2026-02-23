using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "The Email address is not valid.")]
        //[RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@computerpackages\.com$", ErrorMessage = "Please enter a valid CPI e-mail adress")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        //[StringLength(100, ErrorMessage = "The Password must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public bool IsTrue => true;
        [Display(Name = "I have read and agree to the Terms of Service and Privacy Policy")]
        [Compare(nameof(IsTrue), ErrorMessage = "Please check this box to continue.")]
        public bool UserConsent { get; set; }      
    }
}
