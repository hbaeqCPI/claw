using R10.Core.Entities.AMS;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Patent
{

    public class PatCountry:BaseEntity
    {
        public int CountryID { get; set; }

        [Key]
        [Required]
        [StringLength(5)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [StringLength(50)]
        [Display(Name ="Country Name")]
        public string? CountryName { get; set; }

        [StringLength(5)]
        [Display(Name = "CPI Code")]
        public string? CPICode { get; set; }

        public bool CountryPaidThruCPi { get; set; }

        [StringLength(20)]
        [Display(Name = "Tax Schedule Label")]
        public string? LabelTaxSched { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [Display(Name = "Currency Type")]
        public string? CurrencyType { get; set; }

        public List<Client>? CountryClients { get; set; }
        public List<Agent>? CountryAgents { get; set; }
        public List<Owner>? CountryOwners { get; set; }
        public List<Attorney>? CountryAttorneys { get; set; }
        public List<ContactPerson>? CountryContactPersons { get; set; }
        public List<PatInventor>? CountryInventors { get; set; }
        public List<PatInventor>? CitizenshipInventors { get; set; }


        public List<Client>? POCountryClients { get; set; }
        public List<Agent>? POCountryAgents { get; set; }
        public List<Owner>? POCountryOwners { get; set; }
        public List<Attorney>? POCountryAttorneys { get; set; }
        public List<PatInventor>? POCountryInventors { get; set; }

        public List<PatPriority>? CountryPriorities { get; set; }
        public List<PatAreaCountry>? PatCountryAreas { get; set; }
        public List<PatActionType>? PatActionTypes { get; set; }

        public List<CountryApplication>? CountryApplications { get; set; }
        public List<PatActionDue>? PatActionsDue { get; set; }
        public List<PatCountryLaw>? PatCountryLaws { get; set; }

        public List<PatDesCaseType>? ParentPatDesCaseTypes { get; set; }
        public List<PatDesCaseType>? ChildPatDesCaseTypes { get; set; }

        public List<PatCostTrack>? PatCostTrackings { get; set; }
        public List<PatDesignatedCountry>? PatDesignatedCountries { get; set; }

        public List<PatTaxBase>? PatTaxBases { get; set; }

        public List<ClientDesignatedCountry>? ClientDesignatedCountries { get; set; }

        public List<AMSMain>? AMSMain { get; set; }
        public List<AMSVATRate>? AMSVATRate { get; set; }

        public List<FFDueCountry>? FFDueDesCountry { get; set; }
        public List<FFReminderSetup>? FFReminderSetups { get; set; }

        public List<PatInventorAwardCriteria>? PatInventorAwardCriterias { get; set; }
        
        public List<PatBudgetManagement>? PatBudgetManagements { get; set; }

        public List<PatCECountrySetup>? PatCECountrySetups { get; set; }

        public List<PatCEAnnuitySetup>? PatCEAnnuitySetups { get; set; }
        public List<PatCostEstimatorCountry>? PatCostEstimatorCountries { get; set; }
    }
}
