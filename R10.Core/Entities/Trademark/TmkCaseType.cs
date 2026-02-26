using R10.Core.Entities.Patent;
// using R10.Core.Entities.RMS; // Removed during deep clean
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{
    public class TmkCaseType : BaseEntity
    {
        public int CaseTypeId { get; set; }

        [Key]
        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool? LockTmkRecord { get; set; }

        public List<TmkCountryLaw>? CaseTypeCountryLaws { get; set; }

        public List<TmkTrademark>? CaseTypeTrademark { get; set; }

        public List<TmkDesCaseType>? ParentTmkDesCaseTypes { get; set; }
        public List<TmkDesCaseType>? ChildTmkDesCaseTypes { get; set; }
//         public List<RMSReminderSetup>? RMSReminderSetups { get; set; } // Removed during deep clean

        public List<TmkCECountrySetup>? CaseTypeCECountrySetups { get; set; }
    }
}
