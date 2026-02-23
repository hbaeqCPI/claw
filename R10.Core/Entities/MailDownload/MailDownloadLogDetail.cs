using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.MailDownload
{
    public class MailDownloadLogDetail : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LogId { get; set; }

        public int RuleId { get; set; }

        public int ActionId { get; set; }

        public string? DocumentLink { get; set; }

        //Message.InternetMessageId is used as MailId to link log detail to original email.
        //Message.Id changes when moved to different folder
        public string? MailId { get; set; }

        public string? MailFromAddress { get; set; }

        public string? MailToRecipients { get; set; }

        public string? MailSubject { get; set; }

        public DateTime? MailReceivedDate { get; set; }

        public bool? HasAttachments { get; set; }


        public MailDownloadLog? Log { get; set; }
        public MailDownloadRule? Rule { get; set; }
    }
}
