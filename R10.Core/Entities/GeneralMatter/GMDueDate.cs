using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMDueDate : GMDueDateDetail
    {
        public GMActionDue? GMActionDue { get; set; }
        public GMIndicator? GMIndicator { get; set; }

        public List<GMDueDateDeDocket>? DueDateDeDockets { get; set; }
        public GMDueDateDeDocketOutstanding? DeDocketOutstanding { get; set; }
        public Attorney? DueDateAttorney { get; set; }
        public List<GMDueDateDelegation>? Delegations { get; set; }
        public List<GMDueDateDateTakenLog>? DateTakenLogs { get; set; }
        public List<GMDueDateExtension>? GMDueDateExtensions { get; set; }
    }

    public class GMDueDateDetail : BaseEntity
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
