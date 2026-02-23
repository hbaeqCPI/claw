using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.DMS
{
   

    public class DMSDueDate : DMSDueDateDetail
    {
        public DMSActionDue? DMSActionDue { get; set; }

        public DMSIndicator? DMSIndicator { get; set; }

        public Attorney? DueDateAttorney { get; set; }
        public List<DMSDueDateDelegation>? Delegations { get; set; }
        public List<DMSDueDateDateTakenLog>? DateTakenLogs { get; set; }
        public List<DMSDueDateExtension>? DMSDueDateExtensions { get; set; }
    }

    public class DMSDueDateDetail: BaseEntity
    {
        [Key]
        public int DDId { get; set; }

        [Required]
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
