using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.Entities.Trademark;
using R10.Core.DTOs;

namespace R10.Web.Areas.Trademark.ViewModels
{
    public class TmkCostEstimatorDetailViewModel:TmkCostEstimator
    {
        public string? CaseNumber { get; set; }
        
        [Display(Name = "Country")]
        public string? Country { get; set; }
        
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Priority Date")]
        public DateTime? PriorityDate { get; set; }

        [Display(Name = "Trademark Status")]
        public string? TrademarkStatus { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [NotMapped]
        public string? TotalCostEstimate { get; set; }

        [NotMapped]
        public List<ChartDTO>? CountryCosts { get; set; }

    }


}
