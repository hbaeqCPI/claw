using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{

    public class TmkDesCaseType:BaseEntity
    {
        [Key]
        public int DesCaseTypeID { get; set; }
        
        [StringLength(5)]
        public string? IntlCode { get; set; }

        [StringLength(3)]
        public string? CaseType { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? DesCountry { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? DesCaseType { get; set; }
        
        [Display(Name = "Default?")]
        public bool DefaultCaseType { get; set; }
        public int? DesCtryFieldID { get; set; }

        [Display(Name = "Gen. App?")]
        public bool? GenApp { get; set; }

        public TmkCountry? ParentCountry { get; set; }
        public TmkCountry? ChildCountry { get; set; }

        public TmkCaseType? ParentCaseType { get; set; }
        public TmkCaseType? ChildCaseType { get; set; }

        [NotMapped]
        public byte[]? ParentTStamp { get; set; }

        [NotMapped]
        public int CountryLawID { get; set; }
    }
}
