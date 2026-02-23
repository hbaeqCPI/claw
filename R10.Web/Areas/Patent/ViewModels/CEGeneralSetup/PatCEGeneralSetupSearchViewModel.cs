using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCEGeneralSetupSearchViewModel
    {
        public int CEGeneralId { get; set; }

        [Display(Name = "Cost Setup")]
        public string? CostSetup { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        
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
