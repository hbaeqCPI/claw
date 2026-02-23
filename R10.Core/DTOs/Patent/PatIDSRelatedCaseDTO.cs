using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using R10.Core.Entities.Patent;

namespace R10.Core.DTOs
{
    public class PatIDSRelatedCaseDTO : PatIDSReference
    {
        public int RelatedAppId { get; set; }
        public int AppIdConnect { get; set; }

        [Display(Name = "Publication Date")]
        public DateTime? RelatedPubDate { get; set; }

        [Display(Name = "Issue Date")]
        public DateTime? RelatedIssDate { get; set; }

        public string? RelatedAppNumber { get; set; }
        public DateTime? RelatedFilDate { get; set; }

        public DateTime? ReferenceDate { get; set; }
        public DateTime? RelatedDateFiled { get; set; }

        public string? RelatedCountryName { get; set; }
        public string? ReferenceSrc { get; set; }

        public bool ReliedUpon { get; set; }
        public bool ActiveSwitch { get; set; }
        public bool HasTranslation { get; set; }

        public string? RelatedFirstNamedInventor { get; set; }

        [StringLength(2)]
        public string? KindCode { get; set; }

        [StringLength(128)]
        public string? RefPages { get; set; }
        public string? Remarks { get; set; }
        public int? FileId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? DateCreated { get; set; }
        public byte[] tStamp { get; set; }

        [StringLength(255)]
        [Display(Name = "New Document")]
        public string? DocFilePath { get; set; }

        [NotMapped]
        public bool IsDirty { get; set; }
        //public bool CopyToFamily { get; set; }

        public string? DocumentName { get; set; }
        public bool ConsideredByExaminer { get; set; }
    }

    public class PatIDSRelatedCaseCopyDTO: PatIDSReference
    {
    }

    public class PatIDSReference
    {
        [Key]
        public int RelatedCasesId { get; set; }
        public int AppId { get; set; }
        public string? MatchTypeUsed { get; set; }
        public string? RelatedCaseNumber { get; set; }

        [Display(Name = "Country")]
        public string? RelatedCountry { get; set; }

        [Display(Name = "Sub Case")]
        public string? RelatedSubCase { get; set; }

        [Display(Name = "Case Type")]
        public string? RelatedCaseType { get; set; }

        [StringLength(20)]
        [Display(Name = "Publication No.")]
        public string? RelatedPubNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Patent No.")]
        public string? RelatedPatNumber { get; set; }

        [Display(Name = "Relationship")]
        [NotMapped]
        public string? Relationship { get; set; }

        public CountryApplication CountryApplication { get; set; }
    }

    public class IDSTotalDTO
    {
        public int AppId { get; set; }
        public int PLAppId { get; set; }
        public int FiledCount { get; set; }
        public int UnfiledCount { get; set; }
        public int XMLCount { get; set; }
        public DateTime? XMLCountLastUpdate { get; set; }
        public string? XMLCountLastUpdateFormatted { get; set; }

        [NotMapped]
        public int FeeRiskStatus { get; set; }  //1=green, 2=yellow,3=red
    }
}
