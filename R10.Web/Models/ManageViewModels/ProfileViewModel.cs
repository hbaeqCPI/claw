using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.ManageViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Language")]
        public string Locale { get; set; } = "en-US";

        //[Required]
        //[EmailAddress]
        //public string Email { get; set; }

        //[Phone(ErrorMessage = "The Phone number is not valid.")]
        //[Display(Name = "Phone number")]
        //public string PhoneNumber { get; set; }

        public string? StatusMessage { get; set; }
    }
}
