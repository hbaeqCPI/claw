using R10.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Shared
{
    public class TradeSecretAuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Encrypted]
        public string? UpdatedFields { get; set; }

        [Encrypted]
        public string? OldValues { get; set; }

        [Encrypted]
        public string? NewValues { get; set; }

        public TradeSecretActivity? TradeSecretActivity { get; set; }
    }
}
