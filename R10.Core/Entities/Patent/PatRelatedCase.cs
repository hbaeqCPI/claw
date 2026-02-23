using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatRelatedCase : BaseEntity
    {
        [Key]
        public int RelatedCasesId { get; set; }

        public int? RelatedAppId { get; set; }

        public int? AppId { get; set; }

        [StringLength(25)]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name="Country")]
        [Required]
        public string RelatedCountry { get; set; }

        [StringLength(8), Display(Name = "Sub Case")]
        public string? RelatedSubCase { get; set; }

        [StringLength(20)]
        [Display(Name = "Patent No.")]
        public string? RelatedPatNumber { get; set; }

        public string? RelationshipType { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public CountryApplication? CountryApplication { get; set; }


    }
}



