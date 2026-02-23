using R10.Core.Entities.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Trademark
{

    public class TmkActionDue : TmkActionDueDetail
    {
        public TmkTrademark TmkTrademark { get; set; }

        public Attorney? Responsible { get; set; }
        public TmkCountry? TmkCountry { get; set; }

        public List<TmkDueDate>? DueDates { get; set; }
        //public List<TmkImageAct>? Images { get; set; }

        [NotMapped]
        public string? FollowUpAction { get; set; }

        [NotMapped]
        public List<DocFolder>? DocFolders { get; set; }
        public List<TmkDueDateDelegation>? Delegations { get; set; }

        [NotMapped]
        public bool CloseDueDates { get; set; }
    }

    public class TmkActionDueDetail : BaseEntity
    {
        [Key]
        public int ActId { get; set; }
        public int TmkId { get; set; }

        [Required]
        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [Required]
        [StringLength(5)]
        public string? Country { get; set; }

        [StringLength(8)]
        public string? SubCase { get; set; }

        [Required, StringLength(60), Display(Name = "Action Type")]
        public string? ActionType { get; set; }


        [Required, Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Response Date")]
        public DateTime? ResponseDate { get; set; }

        [Display(Name = "Action Received Date")]
        public DateTime? VerifyDate { get; set; }

        public bool ComputerGenerated { get; set; }

        public bool? IsElectronic { get; set; }

        [Display(Name = "Responsible")]
        public int? ResponsibleID { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Office Action?")]
        public bool IsOfficeAction { get; set; }
        
        public string? AutoDocketWorkflowStatus { get; set; }

        [Display(Name = "Action Verified By")]
        public string? VerifiedBy { get; set; }
        [Display(Name = "Action Verified Date")]
        public DateTime? DateVerified { get; set; }
        [StringLength(450)]
        public string? VerifierId { get; set; }

        [Display(Name = "Check Docket?")]
        public bool CheckDocket { get; set; }
    }
}
