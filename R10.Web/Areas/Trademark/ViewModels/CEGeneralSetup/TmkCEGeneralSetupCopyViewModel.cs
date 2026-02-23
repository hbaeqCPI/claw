using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkCEGeneralSetupCopyViewModel
    {
        public int CopyCEGeneralId { get; set; }
        
        [Required]
        [Display(Name="Cost Setup")]
        public string? CostSetup { get; set; }        

        [Display(Name = "Costs")]
        public bool CopyCosts { get; set; } = true;
        
    }
}
