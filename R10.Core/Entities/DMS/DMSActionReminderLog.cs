using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using R10.Core.Identity;

namespace R10.Core.Entities.DMS
{
    public class DMSActionReminderLog : DMSActionReminderLogDetail
    {
        public DMSActionDue? DMSActionDue { get; set; }
        
    }
    public class DMSActionReminderLogDetail : BaseEntity
    {
        [Key]
        public int KeyId { get; set; }
        public int? ActId { get; set; }
        public CPiEntityType? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? Email { get; set; }
        public DateTime? SendDate { get; set; }
        public string? Error { get; set; }
    }
}