using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class PatIDSCopyFamilyDTO
    {
        public int AppId { get; set; }
        public string? CaseNumber { get; set; }
        [Display(Name = "Country")]
        public string? Country { get; set; }
        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? ApplicationStatus { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Patent Number")]
        public string? PatNumber { get; set; }

        [Display(Name = "Gen. Action?")]
        [NotMapped]
        public bool GenerateAction { get; set; } = true;

    }

    public class PatIDSMassCopyFamilyDTO: PatIDSCopyFamilyDTO
    {
        //[Display(Name = "Case Type")]
        //public string? CaseType { get; set; }

        //[Display(Name = "Status")]
        //public string? ApplicationStatus { get; set; }

        //[Display(Name = "Application No.")]
        //public string? AppNumber { get; set; }

        //[Display(Name = "Patent Number")]
        //public string? PatNumber { get; set; }
    }

    public class PatIDSCopyFamilyActionDTO
    {
        public int AppId { get; set; }
        public string? Country { get; set; }

        public List<int> RelatedCasesIds { get; set; }

        [Display(Name = "Action to Generate")]
        public string? ActionToGenerate { get; set; }

        [Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Due Date")]
        public string? DueDateFormatted { get; set; }

        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        public string? RecordType { get; set; }
        public List<PatIDSCopyFamilySelectionDTO> Selection { get; set; }
        public int ActParamId { get; set; }
    }

    public class PatIDSCopyFamilySelectionDTO
    {
        public int AppId { get; set; }
        public bool GenAction { get; set; }
    }
}
