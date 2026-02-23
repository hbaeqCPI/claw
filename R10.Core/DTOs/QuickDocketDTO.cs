using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class QuickDocketDTO
    {
        public int DDId { get; set; }

        public int ActId { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Final Date")]
        public DateTime? FinalDate { get; set; }

        public string? CaseNumber { get; set; }

        public string? System { get; set; }

        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }
        public string? AppPatNo { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }
        public DateTime? FilIssDate { get; set; }

        [Display(Name = "Patent/Reg No.")]
        public string? PatRegNumber { get; set; }

        [Display(Name = "Issue/Reg Date")]
        public DateTime? IssRegDate { get; set; }

        public string? Indicator { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        public string? Client { get; set; }

        public string? ClientRef { get; set; }

        public string? Owner { get; set; }

        public string? Attorney1 { get; set; }
        public string? Attorney2 { get; set; }
        public string? Attorney3 { get; set; }
        public string? Attorney4 { get; set; }
        public string? Attorney5 { get; set; }

        public string? Responsible { get; set; }
        public string? DueDateAtty { get; set; }

        public string? Title { get; set; }

        public string? Attorneys { get; set; }

        public string? DueDateRemarks { get; set; }
        public string? ActionDueRemarks { get; set; }
        public string? QDRemarks { get; set; }

        public int? DeDocketId { get; set; }
        public string? Instruction { get; set; }
        public string? DeDocketRemarks { get; set; }
        public string? InstructedBy { get; set; }
        public DateTime? InstructionDate { get; set; }
        public bool? InstructionCompleted { get; set; }
        public byte[]? ddkTStamp { get; set; }

        public string? RespOffice { get; set; }
        public byte[]? tStamp { get; set; }
        public bool Delegated { get; set; }
        public byte[]? ddTStamp { get; set; }
        public bool? DueDateExtended { get; set; }

        [NotMapped]
        public bool CanEditRemarks { get; set; }
        [NotMapped]
        public bool CanViewRemarks { get; set; }
        [NotMapped]
        public bool CanEditInstruction { get; set; }
        [NotMapped]
        public bool CanEditAction { get; set; }
        [NotMapped]
        public bool CanViewInstruction { get; set; }
        [NotMapped]
        public bool CanCompleteInstruction { get; set; }
        [NotMapped]
        public DateTime? DateTaken { get; set; }
        [NotMapped]
        public bool DateTakenDirty { get; set; }
        [NotMapped]
        public bool RemarksDirty { get; set; }
        [NotMapped]
        public bool DeDocketDirty { get; set; }
        [NotMapped]
        public bool CanAddSoftDocket { get; set; }
        [NotMapped]
        public bool CanRequestDocket { get; set; }
        [NotMapped]
        public bool CanDelegate { get; set; }

    }
}
