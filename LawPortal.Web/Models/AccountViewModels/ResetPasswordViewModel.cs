using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Models.AccountViewModels
{
    public class ResetPasswordViewModel
    {
        //[Required]
        //[EmailAddress(ErrorMessage = "The Email address is not valid.")]
        //public string Email { get; set; }

        [Required]
        //[StringLength(100, ErrorMessage = "The Password must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The Password and Confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string UserId { get; set; }
        public string Code { get; set; }
        public DateTime LastPasswordChangeDate { get; set; } = DateTime.Now;
    }
}
