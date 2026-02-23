using R10.Core.Entities.Documents;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.Patent
{

    public class PatActionDueInv : PatActionDueInvDetail
    {
        public Invention? Invention { get; set; }

        public Attorney? Responsible { get; set; }

        public List<PatDueDateInv>? DueDateInvs { get; set; }

        [NotMapped]
        public string? FollowUpAction { get; set; }

        [NotMapped]
        public List<DocFolder>? DocFolders { get; set; }

        //public FFInstrxTypeAction? FFInstrxTypeAction { get; set; }
        public List<PatDueDateInvDelegation>? Delegations { get; set; }

    }

    public class PatActionDueInvDetail : BaseEntity
    {
        [Key]
        public int ActId { get; set; }
        public int InvId { get; set; }

        [Required]
        [StringLength(25)]
        public string CaseNumber { get; set; }

        [Required, Display(Name = "Action Type")]
        [StringLength(60)]
        public string? ActionType { get; set; }

        [Required, Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Response Date")]
        public DateTime? ResponseDate { get; set; }

        [Display(Name = "Action Received Date")]
        public DateTime? VerifyDate { get; set; }

        public bool ComputerGenerated { get; set; }

        public bool? IsElectronic { get; set; }

        public int? ResponsibleID { get; set; }

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
    }
}
