using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSReportLogViewModel
    {
        public int LogId { get; set; }
        [Display(Name = "Client Code")]
        public string? ClientCode { get; set; }
        public string? Sender { get; set; }
        public string? Recipient { get; set; }
        [Display(Name = "Report Type")]
        public string? ReportType { get; set; }
        [Display(Name = "Sent Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime SentDate { get; set; }
        public string? Path { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public string? BatchId { get; set; }
        [Display(Name = "Annuity Code")]
        public string? AnnuityCode { get; set; }
        [Display(Name = "Sent Date2")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime SentDate2 { get; set; }
        [Display(Name = "Report Type2")]
        public string? ReportType2 { get; set; }
        [Display(Name = "Report Type Description")]
        public string? ReportTypeDesc { get; set; }
    }
}
