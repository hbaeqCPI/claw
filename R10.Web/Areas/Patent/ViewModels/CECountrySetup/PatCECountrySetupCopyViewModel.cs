using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCECountrySetupCopyViewModel
    {
        public int CopyCECountryId { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Required]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Entity Status")]
        public string? EntityStatus { get; set; }

        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }


        [Display(Name = "Costs")]
        public bool CopyCosts { get; set; } = true;
        
    }
}
