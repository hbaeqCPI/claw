using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{

    public class DueDateDeDocket : BaseEntity
    {
        [Key]
        public int DeDocketId { get; set; }

        public int DDId { get; set; }

        [StringLength(45)]
        //[Required(ErrorMessage = "Instruction is required.")]
        [Display(Name = "Instruction")]
        public string?  Instruction { get; set; }

        [Display(Name = "De-Docket Remarks")]
        public string?  Remarks { get; set; }

        [Display(Name = "Instructed By")]
        public string?  InstructedBy { get; set; }

        [Display(Name = "Instruction Date")]
        public DateTime? InstructionDate { get; set; }

        [Display(Name = "Instruction Completed")]
        public bool InstructionCompleted { get; set; }

        [Display(Name = "Completed By")]
        public string?  CompletedBy { get; set; }

        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        [StringLength(255)]
        [Display(Name = "New Document")]
        public string? DocFile { get; set; }
        public int? FileId { get; set; }
        public string? DriveItemId { get; set; }

        [Display(Name = "Saved Doc")]
        [NotMapped]
        public string? CurrentDocFile { get; set; }

        [NotMapped]
        public string?  System { get; set; }

        [NotMapped]
        public string?  CompletedDesc { get; set; }

        [NotMapped]
        public bool CanDeDocket { get; set; }

        [NotMapped]
        public bool CanCompleteInstruction { get; set; }

        [NotMapped]
        public int? ActId { get; set; }

        [NotMapped]
        public bool HasNewInstruction { get; set; }

        [NotMapped]
        public string? Indicator { get; set; }

        [NotMapped]
        public bool CanUploadDocument { get; set; } = true;

        [NotMapped]
        public DateTime? DateTaken { get; set; }
    }

    public class DueDateDeDocketOutStanding
    {
        [Key]
        public int DeDocketId { get; set; }
        public int DDId { get; set; }

        [Display(Name = "Instruction")]
        public string?  Instruction { get; set; }

        [Display(Name = "Remarks")]
        public string?  DeDocketRemarks { get; set; }

        [Display(Name = "Instructed By")]
        public string?  InstructedBy { get; set; }

        [Display(Name = "Instruction Date")]
        public DateTime? InstructionDate { get; set; }

        [Display(Name = "Instruction Completed")]
        public bool InstructionCompleted { get; set; }

        [Display(Name = "Completed By")]
        public string?  CompletedBy { get; set; }

        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        public string?  CreatedBy { get; set; }
        public string?  UpdatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? LastUpdate { get; set; }

        [NotMapped]
        public string?  OldInstruction { get; set; }
        [NotMapped]
        public bool OldInstructionCompleted { get; set; }

        public string? DocFile { get; set; }
        public int? FileId { get; set; }
    }
}
