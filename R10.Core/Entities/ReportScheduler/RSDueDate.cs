using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSDueDate
    {
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Base Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Display(Name = "Due Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [Display(Name = "Responsible")]
        public string? Responsible { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Type")]
        public string? Type { get; set; }

        [Display(Name = "Responsible Office")]
        public string? RespOffice { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        public string? SysSrc { get; set; }
    }
}
