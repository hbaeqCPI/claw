using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class PatInventorAwardExportViewModel
    {
        [Display(Name = "Inventor")]
        public string? Inventor { get; set; }

        [Display(Name = "Award Type")]
        public string? AwardType { get; set; }

        [Display(Name = "Amount")]
        public decimal? Amount { get; set; }

        [Display(Name = "Award Date")]
        public DateTime? AwardDate { get; set; }

        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "System")]
        public string? System { get; set; }
    }
}
