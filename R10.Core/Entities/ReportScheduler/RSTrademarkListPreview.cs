using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSTrademarkListPreview
    {
        [Display(Name = "Case Number")]
        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Country Name")]
        public string? CountryName { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Application Number")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Publication Number")]
        public string? PubNumber { get; set; }

        [Display(Name = "Publication Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? PubDate { get; set; }

        [Display(Name = "Registration Number")]
        public string? RegNumber { get; set; }

        [Display(Name = "Registration Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? RegDate { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Responsible Office")]
        public string? RespOffice { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }
    }
}
