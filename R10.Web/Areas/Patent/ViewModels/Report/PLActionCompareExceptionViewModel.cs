using R10.Web.Areas.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PLActionCompareExceptionViewModel : ReportBaseViewModel
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
}
