using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Entities.MailDownload
{
    public class MailDownloadRuleCondition : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RuleId { get; set; }

        public MailDownloadCondition Condition { get; set; }

        public string? Value { get; set; }


        public MailDownloadRule? Rule { get; set; }
    }


    //ToRecipients, CcRecipients, BccToRecipients are not filtereable
    //https://docs.microsoft.com/en-us/previous-versions/office/office-365-api/api/version-2.0/complex-types-for-mail-contacts-calendar#message
    public enum MailDownloadCondition
    {
        None,
        [Display(Name = "From", Description = "the message was from")]
        From,
        [Display(Name = "Subject contains", Description = "the subject contains")]
        SubjectIncludes,
        [Display(Name = "Body contains", Description = "the body contains")]
        BodyIncludes,
        [Display(Name = "Subject or body contains", Description = "the subject or body contains")]
        SubjectOrBodyIncludes
    }
}
