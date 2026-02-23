using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.ReportScheduler
{
    public class RSMatterListPreview
    {
        [Display(Name = "Case Number")]
        public string CaseNumber { get; set; }

        [Display(Name = "Countries")]
        public string Countries { get; set; }

        [Display(Name = "Sub Case")]
        public string SubCase { get; set; }

        [Display(Name = "Effective/Open")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? EffectiveOpenDate { get; set; }

        [Display(Name = "Terminate/End")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime? TerminationEndDate { get; set; }

        [Display(Name = "Attorneys")]
        public string Attorneys { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Responsible Office")]
        public string RespOffice { get; set; }

        [Display(Name = "Matter Type")]
        public string MatterType { get; set; }
    }
}
