using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Models.DashboardViewModels
{
    public class TmcPortfolioByStatusExportViewModel
    {
        [Display(Name = "LabelCaseNumber")]
        public string CaseNumber { get; set; }        

        [Display(Name = "Status")]
        public string ClearanceStatus { get; set; }

        [Display(Name = "Status Date")]
        public DateTime? ClearanceStatusDate { get; set; }

        [Display(Name = "Date Requested")]
        public DateTime? DateRequested { get; set; }

        [Display(Name = "Requestor's Name")]
        public string Requestor { get; set; }

        [Display(Name = "BU/Category")]
        public string ClientCode { get; set; }

        [Display(Name = "Attorney")]
        public string AttorneyCode { get; set; }

        [Display(Name = "Trademark(s)/Tagline")]
        public string TrademarkTagline { get; set; }

        [Display(Name = "Countries")]
        public string Country { get; set; }
    }
}