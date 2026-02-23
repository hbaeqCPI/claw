using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using R10.Core.Identity;

namespace R10.Core.Entities.DMS
{
    public class DMSDueDateDelegation : DMSDueDateDelegationDetail
    {
        public DMSActionDue? DMSActionDue { get; set; }

        public DMSDueDate? DMSDueDate { get; set; }
    }
    public class DMSDueDateDelegationDetail : BaseEntity
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
