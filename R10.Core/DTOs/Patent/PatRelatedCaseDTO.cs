using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Core.DTOs
{
    public class PatRelatedCaseDTO : BaseEntity
    {
        [Key]
        public int RelatedCasesId { get; set; }

        public int? RelatedAppId { get; set; }

        public int? AppId { get; set; }

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
        
        [Display(Name = "Application No.")]
        public string? RelatedAppNumber { get; set; }

        [Display(Name = "Filing Date")]
        public DateTime? RelatedFilDate { get; set; }

        public CountryApplication CountryApplication { get; set; }
    }
}
