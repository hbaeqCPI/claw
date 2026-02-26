// using R10.Core.Entities.AMS; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class Agent: AgentDetail
    {
        public PatCountry? AddressCountry { get; set; }
        public PatCountry? POAddressCountry { get; set; }
        public Language? AgentLanguage { get; set; }

        public List<AgentContact>? AgentContacts { get; set; }
        public List<CountryApplication>? AgentCountryApplications { get; set; }
        public List<TmkTrademark>? AgentTrademarks { get; set; }
        public List<PatCostTrack>? AgentPatCostTrackings { get; set; }
        public List<TmkCostTrack>? AgentTmkCostTrackings { get; set; }
        public List<PatTaxBase>? AgentPatTaxBases { get; set; }
//         public List<GMMatter>? AgentGMMatters { get; set; } // Removed during deep clean
        public List<PatCountryLaw>? AgentPatCountryLaws { get; set; }
        public List<TmkCountryLaw>? AgentTmkCountryLaws { get; set; }
//         public List<AMSMain>? AgentAMSMain { get; set; } // Removed during deep clean
        public List<TmkConflict>? AgentTmkConflicts { get; set; }
        public List<CountryApplication>? TaxAgentCountryApplications { get; set; }
        public List<CountryApplication>? LegalRepresentativeCountryApplications { get; set; }

        public List<AgentCEFee>? AgentCEFees { get; set; }
    }

    public class AgentDetail : BaseEntity
    {
        [Key]
        public int AgentID { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Agent is required.")]
        public string AgentCode { get; set; }

        [StringLength(60)]
        [Required(ErrorMessage = "Agent Name is required.")]
        public string AgentName { get; set; }

        [StringLength(50)]
        public string? Greeting { get; set; }


        [StringLength(50)]
        [Display(Name = "Address")]
        public string? Address1 { get; set; }

        [StringLength(50)]
        public string? Address2 { get; set; }

        [StringLength(50)]
        public string? Address3 { get; set; }

        [StringLength(50)]
        public string? Address4 { get; set; }

        [StringLength(40)]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(50)]
        [Display(Name = "State/Region")]
        public string? State { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal/Zip Code")]
        public string? ZipCode { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(50)]
        [Display(Name = "PO Box Address")]
        public string? POAddress1 { get; set; }

        [StringLength(50)]
        public string? POAddress2 { get; set; }

        [StringLength(50)]
        public string? POAddress3 { get; set; }

        [StringLength(50)]
        public string? POAddress4 { get; set; }

        [StringLength(40)]
        [Display(Name = "City")]
        public string? POCity { get; set; }

        [StringLength(50)]
        [Display(Name = "State/Region")]
        public string? POState { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal/Zip Code")]
        public string? POZipCode { get; set; }

        [StringLength(5)]
        [Display(Name = "Country")]
        public string? POCountry { get; set; }

        [StringLength(10)]
        [Display(Name = "Language")]
        public string? Language { get; set; }

        [StringLength(20)]
        [Display(Name = "Telephone No.")]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Fax No.")]
        public string? FaxNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile Phone No.")]
        public string? MobileNumber { get; set; }

        [StringLength(150)]
        [Display(Name = "EMail")]
        //[EmailAddress(ErrorMessage = "The Email address is not valid.")]
        //ALLOW MULTIPLE EMAIL ADDRESSES
        [MultiEmailAddress(ErrorMessage = "The Email address is not valid.")]
        public string? EMail { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "The Website is not a valid URL.")]
        [Display(Name = "Website")]
        public string? WebSite { get; set; }

        public int? GenAllLetters { get; set; }

        //ams
        [Display(Name = "Send Agent Responsibility")]
        public bool? GenerateAgentResponsibilityLetter { get; set; }

        [StringLength(50)]
        [Display(Name = "Agent Responsibility Email")]
        public string? AgentResponsibilityCoverLetter { get; set; }

        //rms
        [Display(Name = "Send Agent Confirmation")]
        public bool? RMSGenerateAgentResponsibilityLetter { get; set; }

        [StringLength(50)]
        [Display(Name = "Agent Responsibility Email")]
        public string? RMSAgentResponsibilityCoverLetter { get; set; }

        //foreign filing
        [Display(Name = "Send Agent Confirmation")]
        public bool? FFGenerateAgentResponsibilityLetter { get; set; }

        [StringLength(50)]
        [Display(Name = "Agent Responsibility Email")]
        public string? FFAgentResponsibilityCoverLetter { get; set; }

        public string? Remarks { get; set; }

        [Display(Name = "Active?")]
        public bool? IsActive { get; set; } = true;

        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public DateTime? CustomField4 { get; set; }
        public bool? CustomField5 { get; set; }
    }

}
