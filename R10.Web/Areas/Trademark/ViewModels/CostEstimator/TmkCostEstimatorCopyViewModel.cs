using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkCostEstimatorCopyViewModel
    {
        public int CopyKeyId { get; set; }
        
        [Required]
        [Display(Name="Cost Estimator Name")]
        public string? CostEstimatorName { get; set; }        

        [Display(Name = "Countries")]
        public bool CopyCountries { get; set; } = true;

        [Display(Name = "Answers")]
        public bool CopyAnswers { get; set; } = true;

    }
}
