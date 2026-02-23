using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using R10.Core.Identity;

namespace R10.Core.Entities.GeneralMatter
{
    public class GMDueDateDelegation : GMDueDateDelegationDetail
    {
        public GMActionDue? GMActionDue { get; set; }

        public GMDueDate? GMDueDate { get; set; }
    }
    public class GMDueDateDelegationDetail : BaseEntity
    {
        [Key]
        public int DelegationId { get; set; }
        public int? ActId { get; set; }
        public int? DDId { get; set; }

        public int? GroupId { get; set; }

        public string? UserId { get; set; }
        public int NotificationSent { get; set; }
    }
}
