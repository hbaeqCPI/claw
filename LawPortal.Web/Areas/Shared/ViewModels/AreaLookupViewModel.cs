using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class AreaLookupViewModel
    {
        public int AreaID { get; set; }
        [Required]
        [Display(Name = "Area")]
        public string? Area { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}
