using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class CaseTypeLookupViewModel
    {
        [Required(ErrorMessage = "Case Type field is required")]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
