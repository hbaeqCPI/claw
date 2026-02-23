using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.MailDownload
{
    public class MailDownloadLog : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime? DownloadStart { get; set; }

        public DateTime? DownloadEnd { get; set; }

        public int MailCount { get;set; }

        public DateTime? ReceivedDateTimeFrom { get; set; }

        public DateTime? ReceivedDateTimeTo { get; set; }

        public int RuleId { get; set; } //manual download by rule

        public string? MailboxName { get; set; }


        public List<MailDownloadLogDetail>? LogDetails { get; set; }
    }
}
