using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSReportLogDateViewModel
    {
        [Display(Name = "Annuity Code")]
        public string? AnnuityCode { get; set; }
        public string? Sender { get; set; }
        public string? Recipient { get; set; }
        [Display(Name = "Report Type")]
        public string? ReportType { get; set; }
        [Display(Name = "Sent Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime SentDate { get; set; }
    }
}
