using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class DmsPortfolioByStatusExportViewModel
    {
        [Display(Name = "Disclosure Number")]
        public string? DisclosureNumber { get; set; }

        [Display(Name = "Disclosure Title")]
        public string? DisclosureTitle { get; set; }
                
        [Display(Name = "Disclosure Status")]
        public string? DisclosureStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? DisclosureStatusDate { get; set; }

        [Display(Name = "Disclosure Date")]
        public DateTime? DisclosureDate { get; set; }

        [Display(Name = "Client Code")]
        public string? ClientCode { get; set; }

        [Display(Name = "Client Name")]
        public string? ClientName { get; set; }

        [Display(Name = "Inventor(s)")]
        public string? Inventors { get; set; }

        [Display(Name = "Submitted Date")]
        public DateTime? SubmittedDate { get; set; }
    }
}
