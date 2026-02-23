using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities
{  
    public partial class AgentContact : BaseEntity
    {
        [Key]
        public int AgentContactID { get; set; }

        [Display(Name = "Default?")]
        public bool Default { get; set; }

        [Display(Name = "Send Letters?")]
        [UIHint("LetterOptions")]
        public int GenAllLetters { get; set; }

        [StringLength(1)]
        [Display(Name = "Send As")]
        [UIHint("SendAsOptions")]
        public string?  LetterSendAs { get; set; }

        public int AgentID { get; set; }
        public Agent? Agent { get; set; }

        [Required]
        [Display(Name = "Contact")]
        public int ContactID { get; set; }

        [Display(Name = "Contact")]
        public ContactPerson? Contact { get; set; }

        //AMS
        [Display(Name = "AMS Confirmation")]
        public bool? ReceiveAgentResponsibilityLetter { get; set; }
        [Display(Name = "Last Confirmation Sent")]
        public DateTime? LastResponsibilityLetterSentDate { get; set; }

        //RMS
        [Display(Name = "RMS Confirmation")]
        public bool? RMSReceiveAgentResponsibilityLetter { get; set; }

        [Display(Name = "Last Confirmation Sent")]
        public DateTime? RMSLastResponsibilityLetterSentDate { get; set; }

        //Foreign Filing
        [Display(Name = "FF Confirmation")]
        public bool? FFReceiveAgentResponsibilityLetter { get; set; }

        [Display(Name = "Last Confirmation Sent")]
        public DateTime? FFLastResponsibilityLetterSentDate { get; set; }


        [Display(Name = "Pat Contact")]
        public bool? IsPatentContact { get; set; }

        [Display(Name = "Tmk Contact")]
        public bool? IsTrademarkContact { get; set; }

        [Display(Name = "GM Contact")]
        public bool? IsGeneralMatterContact { get; set; }
    }

    public enum AgentResponsibilityOption
    {
        ReceiveAgentResponsibilityLetter,
        RMSReceiveAgentResponsibilityLetter,
        FFReceiveAgentResponsibilityLetter
    }
}
