using LawPortal.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawPortal.Web.Areas.Shared.ViewModels
{
    public class ActionDueViewModel : BaseEntity
    {
        public int ParentId { get; set; }
        public int ActId { get; set; }
        public int DDId { get; set; }

        [Display(Name = "Action Type")]
        [StringLength(60)]
        public string? ActionType { get; set; }

        [Required]
        [Display(Name = "Action Due")]
        [StringLength(60)]
        public string? ActionDue { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Required]
        [Display(Name = "Indicator")]
        public string? Indicator { get; set; }

        [Display(Name = "Date Taken")]
        public DateTime? DateTaken { get; set; }

        [Display(Name = "Due Date Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Responsible")]
        public int? ResponsibleID { get; set; }

        [Display(Name = "Responsible")]
        public string? ResponsibleCode { get; set; }

        [Display(Name = "Responsible")]
        public string? ResponsibleName { get; set; }

        public bool ComputerGenerated { get; set; }

        public bool? IsForVerify { get; set; }

        public DateTime? IsVerifyDate { get; set; }

        public int? JobId_EPDS { get; set; }

        [Display(Name = "Due Date Attorney")]

        public int? AttorneyID { get; set; }
        [Display(Name = "Due Date Attorney")]
        public string? DueDateAttorneyCode { get; set; }

        [Display(Name = "Due Date Attorney")]
        public string? DueDateAttorneyName { get; set; }

        //dedocket
        public int? DeDocketId { get; set; }

        [Display(Name = "Instruction")]
        public string? Instruction { get; set; }
        public string? OldInstruction { get; set; }

        [Display(Name = "De-Docket Remarks")]
        public string? DeDocketRemarks { get; set; }

        [Display(Name = "Instructed By")]
        public string? InstructedBy { get; set; }

        [Display(Name = "Instruction Date")]
        public DateTime? InstructionDate { get; set; }

        [Display(Name = "Completed?")]
        public bool? InstructionCompleted { get; set; }

        public string? DeDocketCreatedBy { get; set; }
        public DateTime? DeDocketDateCreated { get; set; }
        public string? CaseInfo { get; set; }
        public string? Title { get; set; }
        public bool? HasDelegations { get; set; }

        //rms/ff doc management
        public bool? IsRMSInstructable { get; set; }
        public bool? IsFFInstructable { get; set; }
        public string? CaseNumber { get; set; }
        public string? CountryName { get; set; }
        public string? SubCase { get; set; }
        public string? CaseType { get; set; }

        [StringLength(255)]
        [Display(Name = "New Document")]
        public string? DocFile { get; set; }
        public int? FileId { get; set; }

        [Display(Name = "Saved Doc")]
        public string? CurrentDocFile { get; set; }
        public bool? HasDeDocketInstruction { get; set; }
        public bool? HasNewDeDocketInstruction { get; set; }
        public bool? HasNewInstructionCompleted { get; set; }

        public bool DueDateExtended { get; set; } = false;
    }

    public class ActionDueExportViewModel
    {

        public string? ActionType { get; set; }
        public string? ActionDue { get; set; }
        public DateTime? DateTaken { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Indicator { get; set; }
    }
}
