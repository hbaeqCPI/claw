using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class DocumentVerificationActionDTO
    {
        public int? ActId { get; set; }
        public int? DocId { get; set; }

        public string? System { get; set; }

        [Display(Name = "Document Name")]
        public string? DocName { get; set; }

        public string? DocNames { get; set; }
        public int? ParentId { get; set; }

        public string? FamilyId { get; set; }
        public string? UId { get; set; }

        [Display(Name = "Action Type")]
        public string? ActionType { get; set; }

        [Display(Name = "Base Date")]
        public DateTime? BaseDate { get; set; }

        public string? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }

        public string? CaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Sub Case")]
        public string? SubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        public string? Status { get; set; }

        public string? ActionDue { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Indicator { get; set; }

        public string? DocLibrary { get; set; }
    }
}
