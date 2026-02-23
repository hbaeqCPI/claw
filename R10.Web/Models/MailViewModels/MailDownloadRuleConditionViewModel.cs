using R10.Core.Entities.MailDownload;
using System.ComponentModel.DataAnnotations;

namespace R10.Web.Models.MailViewModels
{
    public class MailDownloadRuleConditionViewModel
    {
        public int Id { get; set; }

        [Required]
        public int RuleId { get; set; }

        [Required]
        public MailDownloadCondition Condition { get; set; }

        [Required]
        public string? Value { get; set; }

        public string tStamp { get; set; }
    }
}
