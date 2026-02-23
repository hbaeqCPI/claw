using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCostEstimatorSearchViewModel
    {
        public int KeyId { get; set; }

        [Display(Name = "Cost Estimator Name")]
        public string? Name { get; set; }        

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
