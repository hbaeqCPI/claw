using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatDueDateInv : PatDueDateInvDetail
    {
        public PatActionDueInv? PatActionDueInv { get; set; }

        public PatIndicator? PatIndicator { get; set; }

        public List<PatDueDateInvDeDocket>? DueDateDeDockets { get; set; }
        public PatDueDateInvDeDocketOutstanding? DeDocketOutstanding { get; set; }

        //public FFDue? FFDue { get; set; }
        //public List<FFRemLogDue>? FFRemLogDues { get; set; }
        //public List<FFActionCloseLogDue>? FFActionCloseLogDues { get; set; }
        public Attorney? DueDateInvAttorney { get; set; }
        public List<PatDueDateInvDelegation>? Delegations { get; set; }
        public List<PatDueDateInvDateTakenLog>? DateTakenLogs { get; set; }
        //public List<PatCostEstimator>? PatCostEstimators { get; set; }
        public List<PatDueDateInvExtension>? PatDueDateInvExtensions { get; set; }
    }

    public class PatDueDateInvDetail : BaseEntity
    {
        [Key]
        public int DDId { get; set; }

        public int ActId { get; set; }

        [Required, StringLength(60), Display(Name = "Action Due")]
        public string ActionDue { get; set; }

        [Required, Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Required, StringLength(20)]
        public string? Indicator { get; set; }

        [Display(Name = "Date Taken")]
        public DateTime? DateTaken { get; set; }

        public string? Remarks { get; set; }

        public bool? IsForVerify { get; set; }

        public DateTime? IsVerifyDate { get; set; }

        public int? JobId_EPDS { get; set; }
        public int? AttorneyID { get; set; }

    }


}
