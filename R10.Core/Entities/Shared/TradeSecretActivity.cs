using R10.Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.Shared
{
    // activity log
    public class TradeSecretActivity
    {
        [Key]
        public int ActivityId { get; set; }

        // discriminator
        // use values from TradeSecretScreen
        [Required]
        [StringLength(50)]
        public string? ScreenId { get; set; }

        [Required]
        public int RecId { get; set; }

        [Required]
        [StringLength(450)]
        public string? UserId { get; set; }

        // use values from TradeSecretActivityCode
        [Required]
        [StringLength(25)]
        public string? Activity { get; set; }

        [Required]
        public DateTime? ActivityDate { get; set; }

        // use values from TradeSecretScreen
        [StringLength(50)]
        public string? Source { get; set; }

        [Required]
        public int RequestId { get; set; }

        public List<TradeSecretAuditLog>? TradeSecretAuditLogs { get; set; }
    }

    // activity code constants
    public static class TradeSecretActivityCode
    {
        public static string Request => "Request";
        public static string View => "View";
        public static string RedactedView => "RedactedView";
        public static string Create => "Create";
        public static string Update => "Update";
        public static string Delete => "Delete";
        public static string Report => "Report";
        public static string Export => "Export";
        public static string Email => "Email";
        public static string Validate => "Validate";
        public static string TimeOut => "TimeOut";
        public static string Download => "Download";
        public static string Refresh => "Refresh";
    }
}
