using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.DTOs
{
    [Keyless]
    public class DocumentVerificationDTO
    {
        public string? KeyId { get; set; }

        //public int? VerifyId { get; set; }

        public int? ActionTypeID { get; set; }

        public int? DocId { get; set; }        

        public string? System { get; set; }

        [Display(Name = "Document Name")]
        public string? DocName { get; set; }

        public string? DocFileName { get; set; }

        public int? ParentId { get; set; }

        [Display(Name = "Uploaded Date")]
        public DateTime? UploadedDate { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Title")]
        public string? Title { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        public string? Attorneys { get; set; }
        public string? Client { get; set; }
        public string? ClientRef { get; set; }
        public string? Owners { get; set; }
        public string? RespOffice { get; set; }

        public string? RespDocketing { get; set; }
        public string? RespReporting { get; set; }

        public string? DriveItemId { get; set; }
        public string? DocLibrary { get; set; }

        public DateTime? AssignedDate { get; set; }

        public int ActId { get; set; }
        public int DDId { get; set; }
        public DateTime? BaseDate { get; set; }
        public string? ActionDue { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Indicator { get; set; }
        public string? Instruction { get; set; }
        public string? DeDocketRemarks { get; set; }
        public string? InstructedBy { get; set; }
        public DateTime? InstructionDate { get; set; }
        public bool? InstructionCompleted { get; set; }

        public string? DocketRequest_CreatedBy { get; set; }
        public DateTime? DocketRequest_DateCreated { get; set; }
        public string? DocketRequest_Remarks { get; set; }

        [NotMapped]
        public bool CanViewRemarks { get; set; }

        [NotMapped]
        public bool CanViewInstruction { get; set; }
        [NotMapped]
        public bool CanCompleteInstruction { get; set; }

        [NotMapped]
        public bool CanViewDocketRequest { get; set; }

        [NotMapped]
        public bool CanUploadDocument { get; set; }
    }    
}
