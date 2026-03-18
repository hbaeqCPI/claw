using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Trademark
{
    public class TmkCountryLaw : BaseEntity
    {
        [NotMapped]
        public string? CopyOptions { get; set; }

        [Key]
        public int CountryLawID { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(3)]
        [Display(Name = "Case Type")]
        public string? CaseType { get; set; }

        [Display(Name = "Agent")]
        public int? DefaultAgent { get; set; }

        public short AutoGenDesCtry { get; set; }

        public short AutoUpdtDesTmkRecs { get; set; }

        public string? Remarks { get; set; }

        public string? UserRemarks { get; set; }

        [StringLength(500)]
        [Display(Name = "Systems")]
        public string? Systems { get; set; }

        public TmkCaseType? TmkCaseType { get; set; }
        public TmkCountry? TmkCountry { get; set; }
        public List<TmkCountryDue>? TmkCountryDues { get; set; }
    }
}
