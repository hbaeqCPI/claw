using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatPriority : BaseEntity
    {
        [Key]
        public int PriId { get; set; }
        
        [Required]
        public int InvId { get; set; }
                
        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }        
        
        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [StringLength(20)]
        [Display(Name = "Application No.")]
        public string? AppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? FilDate { get; set; }

        public int ParentAppId { get; set; }

        [StringLength(12)]
        [Display(Name = "Access Code")]
        public string? AccessCode { get; set; }

        public PatCountry? PriorityCountry { get; set; }
        public PatCaseType? PriorityCaseType { get; set; }
        public Invention? Invention { get; set; }

        public string? AppNumberSearch { get; set; }
    }
}
