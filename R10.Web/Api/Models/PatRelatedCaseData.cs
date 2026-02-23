using System.ComponentModel.DataAnnotations;

namespace R10.Web.Api.Models
{
    public class PatRelatedCaseData
    {
        public int? AppId { get; set; }

        public int? RelatedAppId { get; set; }

        [StringLength(25)]
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Country")]
        [Required]
        public string? RelatedCountry { get; set; }

        [StringLength(8), Display(Name = "Sub Case")]
        public string? RelatedSubCase { get; set; }

        [StringLength(20)]
        [Display(Name = "Patent No.")]
        public string? RelatedPatNumber { get; set; }


        [Display(Name = "Relationship")]
        public string? RelationshipType { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }


        [Display(Name = "Case Type")]
        public string? RelatedCaseType { get; set; }

        [Display(Name = "Status")]
        public string? RelatedStatus { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? RelatedIssDate { get; set; }
    }
}
