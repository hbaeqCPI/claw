using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSActionCompareExceptionViewModel
    {
        public int AppId { get; set; }
        public int PLAppId { get; set; }
        [Display( Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }
        [Display(Name = "Application Status")]
        public string? ApplicationStatus { get; set; }
        [Display(Name = "Actions")]
        public string? ActionType_ { get; set; }
        [Display(Name = "Your Base Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? PMSBaseDate { get; set; }
        [Display(Name = "Your Due Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? PMSDueDate { get; set; }
        [Display(Name = "PTO Base Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? PTOBaseDate { get; set; }
        [Display(Name = "PTO Due Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? PTODueDate { get; set; }
    }
}
