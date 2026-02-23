using R10.Core.Entities.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMActionDue : GMActionDueDetail
    {
        public GMMatter? GMMatter { get; set; }
        public Attorney? Responsible { get; set; }

        public List<GMDueDate>? DueDates { get; set; }
        //public List<GMMatterImageAct>? Images { get; set; }

        [NotMapped]
        public string? FollowUpAction { get; set; }

        [NotMapped]
        public List<DocFolder>? DocFolders { get; set; }
        public List<GMDueDateDelegation>? Delegations { get; set; }

        [NotMapped]
        public bool CloseDueDates { get; set; }
    }

    public class GMActionDueDetail : BaseEntity
    {
        [Key]
        public int ActId { get; set; }
        public int MatId { get; set; }

        [Required]
        [StringLength(25)]
        public string? CaseNumber { get; set; }

        [StringLength(8)]
        public string? SubCase { get; set; }

        [Required, Display(Name = "Action Type")]
        [StringLength(60)]
        public string? ActionType { get; set; }

        [Required, Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Response Date")]
        public DateTime? ResponseDate { get; set; }

        public bool ComputerGenerated { get; set; }

        public int? ResponsibleID { get; set; }

        public string? Remarks { get; set; }

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
