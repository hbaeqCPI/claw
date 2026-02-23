using R10.Core.Entities.AMS;
using R10.Core.Entities.DMS;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Helpers;
using R10.Core.Identity;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public class Attorney: AttorneyDetail
    {
        public PatCountry? AddressCountry { get; set; }
        public PatCountry? POAddressCountry { get; set; }
        public Language? AttorneyLanguage { get; set; }
        public List<Client>? PatDefaultAtty1Clients { get; set; }
        public List<Client>? PatDefaultAtty2Clients { get; set; }
        public List<Client>? PatDefaultAtty3Clients { get; set; }
        public List<Client>? PatDefaultAtty4Clients { get; set; }
        public List<Client>? PatDefaultAtty5Clients { get; set; }
        public List<Client>? TmkDefaultAtty1Clients { get; set; }
        public List<Client>? TmkDefaultAtty2Clients { get; set; }
        public List<Client>? TmkDefaultAtty3Clients { get; set; }
        public List<Client>? TmkDefaultAtty4Clients { get; set; }
        public List<Client>? TmkDefaultAtty5Clients { get; set; }
        public List<Invention>? Attorney1Inventions { get; set; }
        public List<Invention>? Attorney2Inventions { get; set; }
        public List<Invention>? Attorney3Inventions { get; set; }
        public List<Invention>? Attorney4Inventions { get; set; }
        public List<Invention>? Attorney5Inventions { get; set; }
        public List<Disclosure>? AttorneyDisclosures { get; set; }
        public List<PatActionType>? AttorneyPatActionTypes { get; set; }
        public List<TmkActionType>? AttorneyTmkActionTypes { get; set; }
        public List<GMActionType>? AttorneyGMActionTypes { get; set; }
        public List<DMSActionType>? AttorneyDMSActionTypes { get; set; }
        public List<DMSActionDue>? AttorneyDMSActionDues { get; set; }
        public List<PatDueDate>? AttorneyPatDueDates { get; set; }
        public List<PatDueDateInv>? AttorneyPatDueDateInvs { get; set; }
        public List<TmkDueDate>? AttorneyTmkDueDates { get; set; }
        public List<GMDueDate>? AttorneyGMDueDates { get; set; }
        public List<DMSDueDate>? AttorneyDMSDueDates { get; set; }

        public List<TmkTrademark>? Attorney1Trademarks { get; set; }
        public List<TmkTrademark>? Attorney2Trademarks { get; set; }
        public List<TmkTrademark>? Attorney3Trademarks { get; set; }
        public List<TmkTrademark>? Attorney4Trademarks { get; set; }
        public List<TmkTrademark>? Attorney5Trademarks { get; set; }

        public List<PatCostTrack>? PatCostTrackBillings { get; set; }
        public List<TmkCostTrack>? TmkCostTrackBillings { get; set; }
        public List<GMCostTrack>? GMCostTrackBillings { get; set; }

        public List<GMMatterAttorney>? GMMatterAttorneys { get; set; }
        public List<AMSMain>? AttorneyAMSMain { get; set; }
        public List<TimeTracker>? AttorneyTimeTrackers { get; set; }

        public List<CPiUserEntityFilter>? EntityFilters { get; set; }
    }

    public class AttorneyDetail : BaseEntity
    {
        [Key]
        public int AttorneyID { get; set; }

        [Required]
        [StringLength(5)]
        [Display(Name = "Attorney")]
        public string AttorneyCode { get; set; }

        [Required]
        [StringLength(60)]
        [Display(Name = "Attorney Name")]
        public string AttorneyName { get; set; }

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

        [Display(Name = "EMail")]
        //[EmailAddress(ErrorMessage = "The Email address is not valid.")]
        //ALLOW MULTIPLE EMAIL ADDRESSES
        [MultiEmailAddress(ErrorMessage = "The Email address is not valid.")]
        public string? EMail { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "The Website is not a valid URL.")]
        [Display(Name = "Website")]
        public string? WebSite { get; set; }

        [StringLength(15)]
        [Display(Name = "Registration Number")]
        public string? RegistrationNumber { get; set; }

        public int? GenAllLetters { get; set; }


        //AMS SETTINGS
        //ENABLED WHEN IsSendRemindersToAttorney = true
        //"CLIENT" SETTING

        [Display(Name = "Send Attorney Confirmation")]
        public bool? GenerateConfirmationLetter { get; set; }
        [StringLength(50)]
        [Display(Name = "Reminder Email")]
        public string? ReminderCoverLetter { get; set; }
        [StringLength(50)]
        [Display(Name = "Pre-Pay Email")]
        public string? PrePayCoverLetter { get; set; }
        [StringLength(50)]
        [Display(Name = "Confirmation Email")]
        public string? ConfirmationCoverLetter { get; set; }
        [Display(Name = "Payment Needed?")]
        public bool? PayBeforeSending { get; set; }

        //"CONTACT" SETTINGS
        [Display(Name = "Receive AMS Reminder Online")]
        public bool? ReceiveReminderOnline { get; set; }
        [Display(Name = "Receive AMS Reminder Report")]
        public bool? ReceiveReminderReport { get; set; }
        [Display(Name = "Receive AMS Confirmation")]
        public bool? ReceiveConfirmationLetter { get; set; }
        [Display(Name = "Last Reminder Sent")]
        public DateTime? LastReminderSentDate { get; set; }
        [Display(Name = "Receive AMS Prepay Reminder")]
        //prepay fields are currently not used
        //prepay reminders are always sent to clients
        public bool? ReceivePrepayReminder { get; set; }
        [Display(Name = "Last Reminder Sent")]
        public DateTime? LastPrepayReminderSentDate { get; set; }
        [Display(Name = "Last Confirmation Sent")]
        public DateTime? LastConfirmationLetterSentDate { get; set; }


        public string? Remarks { get; set; }

        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Active?")]
        public bool? IsActive { get; set; } = true;

        public string? CustomField1 { get; set; }
        public string? CustomField2 { get; set; }
        public string? CustomField3 { get; set; }
        public DateTime? CustomField4 { get; set; }
        public bool? CustomField5 { get; set; }
    }
}


