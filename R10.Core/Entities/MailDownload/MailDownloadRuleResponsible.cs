using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.MailDownload
{
    public class MailDownloadRuleResponsible : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RuleId { get; set; }

        [Required]
        public string Responsible { get; set; }


        public MailDownloadRule? Rule { get; set; }
    }
}
