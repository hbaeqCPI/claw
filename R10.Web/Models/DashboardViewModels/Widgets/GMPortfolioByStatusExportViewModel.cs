using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class GMPortfolioByStatusExportViewModel
    {
        [Display(Name = "Case Number")]
        public string CaseNumber { get; set; }

        //[Display(Name = "Country")]
        //public string Country { get; set; }

        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }

        [Display(Name = "Matter Type")]
        public string MatterType { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Matter Title")]
        public string MatterTitle { get; set; }             

        [Display(Name = "Effective/Open Date")]
        public DateTime? EffectiveOpenDate { get; set; }

        [Display(Name = "Termination/End Date")]
        public DateTime? TerminationEndDate { get; set; }

        [Display(Name = "Result/Royalty Description")]
        public string ResultRoyalty { get; set; }

    }
}
