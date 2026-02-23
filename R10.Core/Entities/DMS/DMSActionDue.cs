using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace R10.Core.Entities.DMS
{
    public class DMSActionDue: DMSActionDueDetail
    {
        public Disclosure? Disclosure { get; set; }

        public Attorney? Responsible { get; set; }

        public List<DMSDueDate>? DueDates { get; set; }
        //public List<DMSImageAct>? Images { get; set; }

        [NotMapped]
        public string? CopyOptions { get; set; }
        public List<DMSDueDateDelegation>? Delegations { get; set; }

        public List<DMSActionReminderLog>? ReminderLogs { get; set; }

        [NotMapped]
        public bool CloseDueDates { get; set; }
    }


    public class DMSActionDueDetail: BaseEntity
    {
        [Key]
        public int ActId { get; set; }

        public int DMSId { get; set; }

        [Required]
        [StringLength(25)]
        public string? DisclosureNumber { get; set; }

        [Required, Display(Name = "Action Type")]
        [StringLength(60)]
        public string? ActionType { get; set; }

        [Required, Display(Name = "Base Date")]
        public DateTime BaseDate { get; set; }

        [Display(Name = "Response Date")]
        public DateTime? ResponseDate { get; set; }

        [Display(Name = "Responsible")]
        public int? ResponsibleID { get; set; }

        public string? Remarks { get; set; }
    }
}
