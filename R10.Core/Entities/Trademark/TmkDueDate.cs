using R10.Core.Entities.Patent;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.Trademark
{
    public class TmkDueDate : TmkDueDateDetail
    {
        public TmkActionDue? TmkActionDue { get; set; }

        public TmkIndicator? TmkIndicator { get; set; }

        public TmkDueDateDeDocketOutstanding? DeDocketOutstanding { get; set; }
        public List<TmkDueDateDeDocket>? DueDateDeDockets { get; set; }

        public RMSDue? RMSDue { get; set; }
        public List<RMSDueDoc>? RMSDueDocs { get; set; }
        public List<RMSRemLogDue>? RMSRemLogDues { get;set; }
        public List<RMSActionCloseLogDue>? RMSActionCloseLogDues { get; set; }
        public Attorney? DueDateAttorney { get; set; }
        public List<TmkDueDateDelegation>? Delegations { get; set; }
        public List<TmkDueDateDateTakenLog>? DateTakenLogs { get; set; }

        public List<TmkCostEstimator>? TmkCostEstimators { get; set; }
        public List<TmkDueDateExtension>? TmkDueDateExtensions { get; set; }
    }

    public class TmkDueDateDetail : BaseEntity
    {
        [Key]
        public int DDId { get; set; }

        public int ActId { get; set; }

        [Required, StringLength(60), Display(Name = "Action Due")]
        public string? ActionDue { get; set; }

        [Required, Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Required, StringLength(20)]
        public string? Indicator { get; set; }

        [Display(Name = "Date Taken")]
        public DateTime? DateTaken { get; set; }

        public string? Remarks { get; set; }
        public int? AttorneyID { get; set; }
    }
}
