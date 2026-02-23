using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;
using R10.Core.DTOs;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class PatCostEstimatorDetailViewModel:PatCostEstimator
    {
        public string? CaseNumber { get; set; }
        
        [Display(Name = "Country")]
        public string? Country { get; set; }
        
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Priority Date")]
        public DateTime? PriorityDate { get; set; }

        [Display(Name = "Application Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        public string? BaseAppUniqueId { get; set; }

        [NotMapped]
        public string? TotalCostEstimate { get; set; }

        [NotMapped]
        public List<ChartDTO>? CountryCosts { get; set; }

    }


}
