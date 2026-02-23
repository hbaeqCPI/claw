using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{

    public class PatDesCaseType:BaseEntity
    {
        [Key]
        public int DesCaseTypeID { get; set; }
        
        [StringLength(5)]
        public string? IntlCode { get; set; }

        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(5)]
        [Display(Name="Country")]
        [Required]
        public string? DesCountry { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        [Required]
        public string? DesCaseType { get; set; }

        [Display(Name = "Default?")]
        public bool DefaultCaseType { get; set; }
        public int? DesCtryFieldID { get; set; }

        [Display(Name = "Generate App?")]
        public bool? GenApp { get; set; }

        public PatCountry? ParentCountry { get; set; }
        public PatCountry? ChildCountry { get; set; }

        public PatCaseType? ParentCaseType{ get; set; }
        public PatCaseType? ChildCaseType { get; set; }

        [NotMapped]
        public int CountryLawID { get; set; }
        [NotMapped]
        public byte[]? ParentTStamp { get; set; }

        public PatCountryLaw? PatCountryLaw { get; set; }
    }
}
