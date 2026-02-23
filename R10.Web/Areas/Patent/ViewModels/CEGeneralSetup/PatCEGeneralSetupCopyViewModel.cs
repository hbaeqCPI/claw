using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCEGeneralSetupCopyViewModel
    {
        public int CopyCEGeneralId { get; set; }
        
        [Required]
        [Display(Name="Cost Setup")]
        public string? CostSetup { get; set; }        

        [Display(Name = "Costs")]
        public bool CopyCosts { get; set; } = true;
        
    }
}
