using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using R10.Core.Identity;

namespace R10.Core.Entities.Trademark
{
    public class TmkDueDateDelegation : TmkDueDateDelegationDetail
    {
        public TmkActionDue? TmkActionDue { get; set; }

        public TmkDueDate? TmkDueDate { get; set; }
    }
    public class TmkDueDateDelegationDetail : BaseEntity
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
