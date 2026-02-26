using R10.Core.Entities.Patent;
// using R10.Core.Entities.RMS; // Removed during deep clean
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Trademark
{

    public class TmkCountry : BaseEntity
    {
        public int CountryID { get; set; }

        [Key]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(50)]
        [Display(Name ="Country Name")]
        public string? CountryName { get; set; }

        [StringLength(5)]
        [Display(Name = "CPI Code")]
        public string? CPICode { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        [Display(Name = "Single-Class Application?")]
        public bool? SingleClassApplication { get; set; }

        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }

        public List<TmkAreaCountry>? TmkCountryAreas { get; set; }
        public List<TmkActionType>? TmkActionTypes { get; set; }
        public List<TmkCountryLaw>? TmkCountryLaws { get; set; }

        public List<TmkDesCaseType>? ParentTmkDesCaseTypes { get; set; }
        public List<TmkDesCaseType>? ChildTmkDesCaseTypes { get; set; }

        public List<TmkTrademark>? TmkTrademarks { get; set; }
        public List<TmkActionDue>? TmkActionsDue { get; set; }
        public List<TmkTrademark>? TmkPrioTrademarks { get; set; }

        public List<TmkCostTrack>? TmkCostTrackings { get; set; }
        public List<TmkDesignatedCountry>? TmkDesignatedCountries { get; set; }
        public List<TmkConflict>? TmkConflicts { get; set; }

        public List<ClientDesignatedCountry>? ClientDesignatedCountries { get; set; }
        public List<TmkBudgetManagement>? TmkBudgetManagements { get; set; }

//         public List<RMSReminderSetup>? RMSReminderSetups { get; set; } // Removed during deep clean

        public List<TmkCECountrySetup>? TmkCECountrySetups { get; set; }
        public List<TmkCostEstimatorCountry>? TmkCostEstimatorCountries { get; set; }

//         public List<RMSDueCountry>? RMSDueCountries { get; set; } // Removed during deep clean
    }
}
