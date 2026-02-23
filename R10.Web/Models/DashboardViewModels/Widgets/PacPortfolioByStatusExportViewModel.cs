using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class PacPortfolioByStatusExportViewModel
    {
        [Display(Name = "LabelCaseNumber")]
        public string CaseNumber { get; set; }        

        [Display(Name = "Status")]
        public string ClearanceStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? ClearanceStatusDate { get; set; }

        [Display(Name = "Clearance Title")]
        public string ClearanceTitle { get; set; }

        [Display(Name = "Date Requested")]
        public DateTime? DateRequested { get; set; }

        [Display(Name = "Requestors")]
        public string Requestors { get; set; }

        [Display(Name = "Division-Segment")]
        public string ClientCode { get; set; }

        [Display(Name = "Attorney")]
        public string AttorneyCode { get; set; }        
    }
}