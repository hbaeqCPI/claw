using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.MailDownload
{
    public class MailDownloadRule : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int MailboxId { get; set; }

        public int ActionId { get; set; }

        public bool Enabled { get; set; } = true;

        [Display(Name = "Download attachments")]
        public bool DownloadAttachments { get;set; }

        public int OrderOfEntry { get; set; }

        /// <summary>
        /// If multiple rules apply to a single message,
        /// stop processing succeeding rules if document link is found
        /// </summary>
        [Display(Name="Stop processing more rules")]
        public bool StopProcessing { get; set; } = true;

        [Display(Name = "Move downloaded mail to folder")]
        public string? DownloadFolderId { get; set; }
        [Display(Name = "Keep downloaded mail in Inbox")]
        public bool DoNotMove { get; set; } = false;

        public MailDownloadAction? Action { get; set; }
        public List<MailDownloadRuleCondition>? RuleConditions { get; set; }
        public List<MailDownloadLogDetail>? LogDetails { get; set; }

        [Display(Name = "Responsible")]
        public List<MailDownloadRuleResponsible>? Responsibles { get; set; }
    }
}
