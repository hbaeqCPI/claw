using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSEmailedReportViewModel
    {
        [Display(Name = "Annuity Code")]
        public string? AnnuityCode { get; set; }
        [Display(Name = "Sender")]
        public string? Sender { get; set; }
        [Display(Name = "Recipient")]
        public string? Recipient { get; set; }
        [Display(Name = "Report Type")]
        public string? ReportType { get; set; }
        [Key]
        [Display(Name = "Sent Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime SentDate { get; set; }
        [Display(Name = "Numbers & Dates Update")]
        public string? Biblio { get; set; }
        public string? Assignment { get; set; }
        [Display(Name = "Compare Report")]
        public string? Compare { get; set; }
    }
}
