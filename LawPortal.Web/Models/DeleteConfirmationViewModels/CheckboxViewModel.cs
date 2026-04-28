using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Models.DeleteConfirmationViewModels
{
    public class CheckboxViewModel
    {
        [Required]
        [Display(Name = "Please check to confirm deletion")]
        public bool Confirm { get; set; }
    }
}
