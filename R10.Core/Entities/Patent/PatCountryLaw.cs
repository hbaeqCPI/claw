using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatCountryLaw: BaseEntity
    {
        [Key]
        public int CountryLawID { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        [Required]
        public string Country { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        [Required]
        public string CaseType { get; set; }

        [Display(Name="Agent")]
        public int? DefaultAgent { get; set; }

        public short AutoGenDesCtry { get; set; }
        public short AutoUpdtDesPatRecs { get; set; }
        public bool CalcExpirBeforeIssue { get; set; }
        public string? Remarks { get; set; }
        public string? UserRemarks { get; set; }

        [StringLength(20)]
        public string? LabelTaxSched { get; set; }

        public PatCaseType? PatCaseType { get; set; }
        public PatCountry? PatCountry { get; set; }
        public Agent? Agent { get; set; }
        public List<PatCountryDue>? PatCountryDues { get; set; }
        public List<CountryApplication>? CountryApplications { get; set; }
        public List<PatDesCaseType>? PatDesCaseTypes { get; set; }
    }
    
}
