using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LawPortal.Core.DTOs
{
    [Keyless]
    public class DocIDSRelCasesDTO
    {
        public int RelatedCasesId { get; set; }

        [Display(Name="Relationship")]
        public string? MatchTypeUsed { get; set; }

        public string? RelatedCaseNumber { get; set; }
        
        [Display(Name="Country")]
        public string? RelatedCountry { get; set; }
        
        [Display(Name="SubCase")]
        public string? RelatedSubCase { get; set; }
        
        [Display(Name="Case Type")]
        public string? RelatedCaseType{ get; set; }
        
        [Display(Name="Publication No.")]
        public string? RelatedPubNumber { get; set; }

        [Display(Name="Publication Date")]
        public DateTime? RelatedPubDate { get; set; }

        [Display(Name="Patent No.")]
        public string? RelatedPatNumber { get; set; }

        [Display(Name="Issue Date")]
        public DateTime? RelatedIssDate { get; set; }

        [Display(Name="Applicant")]
        public string? RelatedFirstNamedInventor { get; set; }

        [Display(Name="IDS File Date")]
        public DateTime? RelatedDateFiled { get; set; }

        [Display(Name="Relied Upon?")]
        public bool ReliedUpon { get; set; }

        [Display(Name = "Reference Source")]
        public string? ReferenceSrc { get; set; }

        [Display(Name = "Reference Date")]
        public DateTime? ReferenceDate { get; set; }

        [Display(Name = "Applicable?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Kind Code")]
        [StringLength(2)]
        public string? KindCode { get; set; }

        [Display(Name = "Reference Pages")]
        [StringLength(128)]
        public string? RefPages { get; set; }

        [Display(Name = "Has Translation?")]
        public bool HasTranslation { get; set; }

        public string? DocUrl { get; set; }
        public string? DocFileName { get; set; }

        [NotMapped]
        public string? DocumentLink { get; set; }
    }
}
