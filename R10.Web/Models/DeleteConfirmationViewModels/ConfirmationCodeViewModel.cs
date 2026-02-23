using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DeleteConfirmationViewModels
{
    public class ConfirmationCodeViewModel
    {
        public string ConfirmationToken { get; set; }
        
        [Required(ErrorMessage = "Please enter code to confirm")]
        [Compare("ConfirmationToken", ErrorMessage = "The code entered is not valid")]
        [Display(Name = "Please enter code to confirm deletion")]
        public string ConfirmationCode { get; set; }
    }
}
