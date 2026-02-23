using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PatIDSRelatedCase : BaseEntity
    {
        [Key]
        public int RelatedCasesId { get; set; }

        public int? RelatedAppId { get; set; }

        public int AppId { get; set; }

        [StringLength(25)]
        public string? RelatedCaseNumber { get; set; }

        [StringLength(5), Display(Name="Country")]
        [Required]
        public string? RelatedCountry { get; set; }

        [StringLength(8), Display(Name = "Sub Case")]
        public string? RelatedSubCase { get; set; }
        
        [Display(Name = "Applicable?")]
        public bool ActiveSwitch { get; set; }

        [Display(Name = "Relationship")]
        public string MatchTypeUsed { get; set; } = "Cited";

        [StringLength(20)]
        public string? RelatedAppNumber { get; set; }

        public DateTime? RelatedFilDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Publication No.")]
        public string? RelatedPubNumber { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? RelatedPubDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Patent No.")]
        public string? RelatedPatNumber { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? RelatedIssDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Applicant Name")]
        public string? RelatedFirstNamedInventor { get; set; }

        [Display(Name = "IDS File Date")]
        public DateTime? RelatedDateFiled { get; set; }

        [StringLength(20)]
        public string? ForeignDate { get; set; }

        [StringLength(20)]
        [Display(Name = "Reference Source")]
        public string? ReferenceSrc { get; set; }

        [Display(Name = "Reference Date")]
        public DateTime? ReferenceDate { get; set; }

        [Display(Name = "Relied Upon")]
        public bool ReliedUpon { get; set; }

        [StringLength(255)]
        public string? DocURL { get; set; }

        [StringLength(255)]
        public string? DocTitle { get; set; }

        [Display(Name = "Kind Code")]
        [StringLength(2)]
        public string? KindCode { get; set; }

        [Display(Name = "Reference Pages")]
        [StringLength(128)]
        public string? RefPages { get; set; }

        [Display(Name = "Has Translation?")]
        public bool HasTranslation { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        public string? DocFilePath { get; set; }
        public int? FileId { get; set; }
        public string? DataSource { get; set; }
        public bool FromParent { get; set; }

        [Display(Name = "Relationship")]
        [NotMapped]
        public string? Relationship { get; set; }

        [Display(Name = "Case Type")]
        [NotMapped]
        public string? RelatedCaseType { get; set; }

        [NotMapped]
        public int? AppIDConnect { get; set; }

        [NotMapped]
        public string? CurrentDocFile { get; set; }
        
        public string? ExaminerDocDate { get; set; }
        public string? CPCClass { get; set; }
        public string? USClass { get; set; }
        
        [NotMapped]
        public string? ExaminerPubDate { get; set; }
        [NotMapped]
        public string? ExaminerIssDate { get; set; }

        [NotMapped]
        public bool WithFee { get; set; }

        [NotMapped]
        public bool PossibleDuplicate { get; set; }

        public CountryApplication? CountryApplication { get; set; }
        public DateTime? MetaUpdate { get; set; } = DateTime.Now;           // Azure blob storage metadata update date

        [Display(Name = "Considered By Examiner?")]
        public bool ConsideredByExaminer { get; set; }

        public string? RelatedPubNumberStandard { get; set; }
        public string? RelatedPatNumberStandard { get; set; }
        public string? RelatedPubNumberStandardYear { get; set; }

        // public CountryApplication RelatedCountryApplication { get; set; }

    }
}



