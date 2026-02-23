using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCEAnnuitySetupSearchViewModel
    {
        public int CEAnnuityId { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Case Type(s)")]
        public string? CaseTypes { get; set; }

        [Display(Name = "Entity Status")]
        public string? EntityStatus { get; set; }

        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }

        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Created On")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Updated On")]
        public DateTime? LastUpdate { get; set; }
        
    }
}
