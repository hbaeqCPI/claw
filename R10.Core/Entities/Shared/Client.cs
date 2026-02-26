// using R10.Core.Entities.AMS; // Removed during deep clean
// using R10.Core.Entities.Clearance; // Removed during deep clean
// using R10.Core.Entities.DMS; // Removed during deep clean
// using R10.Core.Entities.GeneralMatter; // Removed during deep clean
// using R10.Core.Entities.PatClearance; // Removed during deep clean
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities
{
    public class Client: ClientDetail
    {
        public PatCountry? AddressCountry { get; set; }
        public PatCountry? POAddressCountry { get; set; }
        public Attorney? PatDefaultAtty1 { get; set; }
        public Attorney? PatDefaultAtty2 { get; set; }
        public Attorney? PatDefaultAtty3 { get; set; }
        public Attorney? PatDefaultAtty4 { get; set; }
        public Attorney? PatDefaultAtty5 { get; set; }
        public Attorney? TmkDefaultAtty1 { get; set; }
        public Attorney? TmkDefaultAtty2 { get; set; }
        public Attorney? TmkDefaultAtty3 { get; set; }
        public Attorney? TmkDefaultAtty4 { get; set; }
        public Attorney? TmkDefaultAtty5 { get; set; }
        public Language? ClientLanguage { get; set; }
//         public AMSFee? AMSFee { get; set; } // Removed during deep clean

        public List<ClientContact>? ClientContacts { get; set; }
        public List<ClientDesignatedCountry>? ClientDesignatedCountries { get; set; }
        public List<Invention>? ClientInventions { get; set; }
//         public List<Disclosure>? ClientDisclosures { get; set; } // Removed during deep clean
//         public List<DMSAgenda>? ClientDMSAgendas { get; set; } // Removed during deep clean
        public List<TmkTrademark>? ClientTrademarks { get; set; }
//         public List<GMMatter>? ClientGMMatters { get; set; } // Removed during deep clean
//         public List<AMSMain>? ClientAMSMain { get; set; } // Removed during deep clean
//         public List<DMSEntityReviewer>? Reviewers { get; set; } // Removed during deep clean

//         public List<TmcClearance>? ClientClearances { get; set; } // Removed during deep clean
//         public List<PacClearance>? ClientPacClearances { get; set; } // Removed during deep clean
//         public List<RTSMapActionDocumentClient>? RTSMapActionDocumentClients { get; set; } // Removed during deep clean
//         public List<TLMapActionDocumentClient>? TLMapActionDocumentClients { get; set; } // Removed during deep clean

        public PatCEFee? PatCEFee { get; set; }
        public PatCEGeneralSetup? PatCEGeneralSetup { get; set; }
        public TmkCEFee? TmkCEFee { get; set; }
        public TmkCEGeneralSetup? TmkCEGeneralSetup { get; set; }

        public PatIRRemunerationFormula? RemunerationSetting { get; set; }
        
    }

    public class ClientDetail : BaseEntity
    {
        [Key]
        public int ClientID { get; set; }

        [Display(Name = "Client")] 
        [Required(ErrorMessage ="Client is required.")]
        [StringLength(10)]
        public string ClientCode { get; set; }

        [Display(Name = "Client Name")]
        [Required(ErrorMessage = "Client Name is required.")]
        [StringLength(60)]
        public string ClientName { get; set; }

        [StringLength(50)]
        [Display(Name ="Address")]
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

        //[Display(Name = "Patent Attorney 1")]
        public int? PatAttorney1ID { get; set; }
        //[Display(Name = "Patent Attorney 2")]
        public int? PatAttorney2ID { get; set; }
        //[Display(Name = "Patent Attorney 3")]
        public int? PatAttorney3ID { get; set; }
        public int? PatAttorney4ID { get; set; }
        public int? PatAttorney5ID { get; set; }

        //[Display(Name = "Trademark Attorney 1")]
        public int? TmkAttorney1ID { get; set; }
        //[Display(Name = "Trademark Attorney 2")]
        public int? TmkAttorney2ID { get; set; }
        //[Display(Name = "Trademark Attorney 3")]
        public int? TmkAttorney3ID { get; set; }
        public int? TmkAttorney4ID { get; set; }
        public int? TmkAttorney5ID { get; set; }

        [StringLength(50)]
        [Display(Name = "Address")]
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

        [StringLength(3)]
        [Display(Name = "Tax Schedule")]
        public string? TaxSchedule { get; set; }

        public int? GenAllLetters { get; set; }

        [Display(Name = "Client Paid Thru CPi")]
        public bool? ClientPaidThruCPi { get; set; }

        //ams
        [Display(Name = "Use VAT")]
        public bool? UseVAT { get; set; }   //NOT USED??

        [StringLength(5)]
        [Display(Name = "VAT Country")]
        public string? VATCountry { get; set; }

        [Display(Name = "Use Decision Management")]
        public bool? UseDecisionMgt { get; set; }

        [StringLength(50)]
        [Display(Name = "Reminder Email")]
        public string? ReminderCoverLetter { get; set; }

        [StringLength(50)]
        [Display(Name = "Pre-Pay Email")]
        public string? PrePayCoverLetter { get; set; }

        [Display(Name = "Send Client Confirmation")]
        public bool? GenerateConfirmationLetter { get; set; }

        [StringLength(50)]
        [Display(Name = "Confirmation Email")]
        public string? ConfirmationCoverLetter { get; set; }

        [StringLength(10)]
        [Display(Name = "Fee Setup")]
        public string? FeeSetupName { get; set; }

        [Display(Name = "Payment Needed?")]
        public bool? PayBeforeSending { get; set; }

        [Display(Name = "Send Attorney Reminder Summary")]
        public bool? GenerateCcRemToAtty { get; set; }

        [StringLength(50)]
        [Display(Name = "Reminder Summary Email")]
        public string? AttorneySummaryCoverLetter { get; set; }

        [Display(Name = "Use Decision Management")]
        public bool? RMSUseDecisionMgt { get; set; }

        [StringLength(50)]
        [Display(Name = "Reminder Email")]
        public string? RMSReminderCoverLetter { get; set; }

        [Display(Name = "Send Client Confirmation")]
        public bool? RMSGenerateConfirmationLetter { get; set; }

        [StringLength(50)]
        [Display(Name = "Confirmation Email")]
        public string? RMSConfirmationCoverLetter { get; set; }

        [Display(Name = "Payment Needed?")]
        public bool? RMSPayBeforeSending { get; set; }

        [StringLength(50)]
        [Display(Name = "Reminder Email")]
        public string? FFReminderCoverLetter { get; set; }

        [Display(Name = "Send Client Confirmation")]
        public bool? FFGenerateConfirmationLetter { get; set; }

        [StringLength(50)]
        [Display(Name = "Confirmation Email")]
        public string? FFConfirmationCoverLetter { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Active?")]
        public bool? IsActive { get; set; } = true;

        [StringLength(10)]
        [Display(Name = "Cost Estimator Fee Setup")]
        public string? CEFeeSetupName { get; set; }

        [Display(Name = "Show Cost To Expiration")]
        public bool? ShowCostToExpiration { get; set; }

        public int? PatCEGeneralId { get; set; }
        public int? RemunerationSettingId { get; set; }

        [StringLength(10)]
        [Display(Name = "Cost Estimator Fee Setup")]
        public string? TmkCEFeeSetupName { get; set; }
        public int? TmkCEGeneralId { get; set; }

        [Display(Name = "Use In Disclosure")]
        public bool? UseInDMS { get; set; } = false;

        [Display(Name = "Days")]
        public int? DueDateExtendDay { get; set; } = 0;

        [Display(Name = "Weeks")]
        public int? DueDateExtendWeek { get; set; } = 0;

        [Display(Name = "Months")]
        public int? DueDateExtendMonth { get; set; } = 0;

        [Display(Name = "Repeat every")]
        public int? DueDateExtendRepeatInterval { get; set; } = 0;

        public string? DueDateExtendRepeatRecurrence { get; set; } = "D";

        [Display(Name = "Repeat on")]
        public int? DueDateExtendRepeatOnDay { get; set; } = 1;

        [Display(Name = "Ends")]
        public string? DueDateExtendStopIndicator { get; set; } = "N";

        [Display(Name = "occurences")]
        public int? DueDateExtendStopAfterCount { get; set; } = 1;

        [Display(Name = "Stop Date")]
        public DateTime? DueDateExtendStopDate { get; set; }

        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public DateTime? CustomField4 { get; set; }
        public bool? CustomField5 { get; set; }
        
    }
}
