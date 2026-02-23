using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Trademark.ViewModels.Report
{
    public class TLActionCompareExceptionReportViewModel : ReportBaseViewModel
    {
        public int CompareOptions { get; set; }
        public int ActionCutOffMonths { get; set; }
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }
        public string? RespOffice { get; set; }
        public string? RespOffices { get; set; }
    }

    public class TLActionCompareExceptionViewModel
    {
        public int TmkId { get; set; }
        public int TLTmkId { get; set; }
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }
        [Display(Name = "Trademark Status")]
        public string? TrademarkStatus { get; set; }
        [Display(Name = "Actions")]
        public string? ActionType_ { get; set; }
        [Display(Name = "Your Base Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? TMSBaseDate { get; set; }
        [Display(Name = "Your Due Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? TMSDueDate { get; set; }
        [Display(Name = "PTO Base Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? PTOBaseDate { get; set; }
        [Display(Name = "PTO Due Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? PTODueDate { get; set; }
    }

}
