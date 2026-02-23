using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{
    public partial class ClientContact : BaseEntity
    {
        [Key]
        public int ClientContactID { get; set; }

        [Display(Name = "Default?")]
        public bool Default { get; set; }

        [Display(Name = "Send Letters?")]
        [UIHint("LetterOptions")]
        public int GenAllLetters { get; set; }

        [StringLength(1)]
        [Display(Name = "Send As")]
        [UIHint("SendAsOptions")]
        public string?  LetterSendAs { get; set; }

        public int ClientID { get; set; }
        public Client? Client { get; set; }

        [Required]
        [Display(Name = "Contact")]
        public int ContactID { get; set; }

        [Display(Name = "Contact")]
        public ContactPerson? Contact { get; set; }

        //AMS
        [Display(Name = "AMS Reminder Online")]
        public bool? ReceiveReminderOnline { get; set; }

        [Display(Name = "AMS Reminder Report")]
        public bool? ReceiveReminderReport { get; set; }

        [Display(Name = "AMS Prepay Reminder")]
        public bool? ReceivePrepayReminder { get; set; }

        [Display(Name = "Last Reminder Sent")]
        public DateTime? LastReminderSentDate { get; set; }

        [Display(Name = "Last Reminder Sent")]
        public DateTime? LastPrepayReminderSentDate { get; set; }

        [Display(Name = "AMS Confirmation")]
        public bool? ReceiveConfirmationLetter { get; set; }

        [Display(Name = "Last Confirmation Sent")]
        public DateTime? LastConfirmationLetterSentDate { get; set; }

        [Display(Name = "AMS Decision Maker")]
        public bool? IsDecisionMaker { get; set; }

        //RMS
        [Display(Name = "RMS Reminder Online")]
        public bool? RMSReceiveReminder { get; set; }

        [Display(Name = "RMS Reminder Report")]
        public bool? RMSReceiveReminderReport { get; set; }

        [Display(Name = "Last Reminder Sent")]
        public DateTime? RMSLastReminderSentDate { get; set; }

        [Display(Name = "RMS Confirmation")]
        public bool? RMSReceiveConfirmationLetter { get; set; }

        [Display(Name = "Last Confirmation Sent")]
        public DateTime? RMSLastConfirmationLetterSentDate { get; set; }

        [Display(Name = "RMS Decision Maker")]
        public bool? RMSIsDecisionMaker { get; set; }

        //Foreign Filing
        [Display(Name = "FF Reminder Online")]
        public bool? FFReceiveReminder { get; set; }

        [Display(Name = "FF Reminder Report")]
        public bool? FFReceiveReminderReport { get; set; }

        [Display(Name = "Last Reminder Sent")]
        public DateTime? FFLastReminderSentDate { get; set; }

        [Display(Name = "FF Confirmation")]
        public bool? FFReceiveConfirmationLetter { get; set; }

        [Display(Name = "Last Confirmation Sent")]
        public DateTime? FFLastConfirmationLetterSentDate { get; set; }


        [Display(Name = "Pat Contact")]
        public bool? IsPatentContact { get; set; }

        [Display(Name = "Tmk Contact")]
        public bool? IsTrademarkContact { get; set; }

        [Display(Name = "GM Contact")]
        public bool? IsGeneralMatterContact { get; set; }
    }

    public enum ReminderOption
    {
        ReceiveReminderOnline,
        ReceiveReminderReport,
        ReceivePrepayReminder,
        ReceiveRMSReminder,
        ReceiveRMSReminderReport,
        ReceiveFFReminder,
        ReceiveFFReminderReport
    }
}
